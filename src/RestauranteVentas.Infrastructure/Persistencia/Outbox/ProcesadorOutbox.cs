using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RestauranteVentas.Infrastructure.Persistencia.Outbox;

/// <summary>
/// Consumidor local del outbox. Reclama de forma atómica los mensajes
/// pendientes, escribe una auditoría estructurada y los marca como
/// procesados. El identificador estable del evento permite que futuros
/// consumidores externos mantengan la misma semántica idempotente.
/// </summary>
public sealed class ProcesadorOutbox(
    IServiceScopeFactory fabricaAlcances,
    ILogger<ProcesadorOutbox> registrador) : BackgroundService
{
    private const int TamanoLote = 20;
    private static readonly TimeSpan IntervaloSondeo = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarLoteAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception excepcion)
            {
                registrador.LogError(excepcion, "No fue posible procesar los mensajes pendientes del outbox.");
            }

            try
            {
                await Task.Delay(IntervaloSondeo, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcesarLoteAsync(CancellationToken cancellationToken)
    {
        await using var alcance = fabricaAlcances.CreateAsyncScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<RestauranteVentasDbContext>();
        var ahoraUtc = DateTime.UtcNow;

        var idsPendientes = await contexto.MensajesOutbox
            .AsNoTracking()
            .Where(mensaje => mensaje.ProcesadoEnUtc == null &&
                (mensaje.BloqueadoHastaUtc == null || mensaje.BloqueadoHastaUtc <= ahoraUtc))
            .OrderBy(mensaje => mensaje.OcurridoEnUtc)
            .ThenBy(mensaje => mensaje.Id)
            .Select(mensaje => mensaje.Id)
            .Take(TamanoLote)
            .ToListAsync(cancellationToken);

        foreach (var idMensaje in idsPendientes)
        {
            await ProcesarMensajeAsync(contexto, idMensaje, cancellationToken);
        }
    }

    private async Task ProcesarMensajeAsync(
        RestauranteVentasDbContext contexto,
        Guid idMensaje,
        CancellationToken cancellationToken)
    {
        var tokenBloqueo = Guid.NewGuid();
        var ahoraUtc = DateTime.UtcNow;
        var bloqueadoHastaUtc = ahoraUtc.Add(DuracionBloqueo);

        var fueReclamado = await contexto.MensajesOutbox
            .Where(mensaje => mensaje.Id == idMensaje &&
                mensaje.ProcesadoEnUtc == null &&
                (mensaje.BloqueadoHastaUtc == null || mensaje.BloqueadoHastaUtc <= ahoraUtc))
            .ExecuteUpdateAsync(
                actualizador => actualizador
                    .SetProperty(mensaje => mensaje.BloqueadoHastaUtc, bloqueadoHastaUtc)
                    .SetProperty(mensaje => mensaje.TokenBloqueo, tokenBloqueo),
                cancellationToken);

        if (fueReclamado == 0)
        {
            return;
        }

        var mensaje = await contexto.MensajesOutbox.SingleOrDefaultAsync(
            mensaje => mensaje.Id == idMensaje && mensaje.TokenBloqueo == tokenBloqueo,
            cancellationToken);

        if (mensaje is null)
        {
            return;
        }

        try
        {
            // Este es un consumidor real y observable: deja una traza de
            // auditoría estructurada por cada hecho de dominio entregado.
            registrador.LogInformation(
                "Evento de dominio procesado por outbox. {EventoId} {TipoEvento} {OcurridoEnUtc} {Intentos}",
                mensaje.Id,
                mensaje.TipoEvento,
                mensaje.OcurridoEnUtc,
                mensaje.Intentos);

            var fueMarcadoComoProcesado = await MarcarComoProcesadoAsync(
                contexto,
                idMensaje,
                tokenBloqueo,
                DateTime.UtcNow,
                cancellationToken);

            if (!fueMarcadoComoProcesado)
            {
                registrador.LogWarning(
                    "El lease del mensaje de outbox {EventoId} cambió antes de confirmar el procesamiento.",
                    idMensaje);
            }
        }
        catch (Exception excepcion)
        {
            var proximoIntentoUtc = DateTime.UtcNow.Add(CalcularEsperaReintento(mensaje.Intentos));
            var fueMarcadoParaReintento = await MarcarParaReintentoAsync(
                contexto,
                idMensaje,
                tokenBloqueo,
                excepcion.ToString(),
                proximoIntentoUtc,
                cancellationToken);

            registrador.LogError(
                excepcion,
                "No fue posible procesar el mensaje de outbox {EventoId}; se reintentará en {ProximoIntentoUtc}.",
                mensaje.Id,
                proximoIntentoUtc);

            if (!fueMarcadoParaReintento)
            {
                registrador.LogWarning(
                    "El lease del mensaje de outbox {EventoId} cambió antes de registrar el reintento.",
                    idMensaje);
            }
        }
    }

    private static async Task<bool> MarcarComoProcesadoAsync(
        RestauranteVentasDbContext contexto,
        Guid idMensaje,
        Guid tokenBloqueo,
        DateTime procesadoEnUtc,
        CancellationToken cancellationToken)
    {
        var filasActualizadas = await MensajeReclamadoPor(contexto, idMensaje, tokenBloqueo)
            .ExecuteUpdateAsync(
                actualizador => actualizador
                    .SetProperty(mensaje => mensaje.ProcesadoEnUtc, procesadoEnUtc)
                    .SetProperty(mensaje => mensaje.BloqueadoHastaUtc, (DateTime?)null)
                    .SetProperty(mensaje => mensaje.TokenBloqueo, (Guid?)null)
                    .SetProperty(mensaje => mensaje.ErrorProcesamiento, (string?)null),
                cancellationToken);

        return filasActualizadas == 1;
    }

    private static async Task<bool> MarcarParaReintentoAsync(
        RestauranteVentasDbContext contexto,
        Guid idMensaje,
        Guid tokenBloqueo,
        string errorProcesamiento,
        DateTime proximoIntentoUtc,
        CancellationToken cancellationToken)
    {
        var errorNormalizado = errorProcesamiento.Length <= OutboxMensaje.LongitudMaximaErrorProcesamiento
            ? errorProcesamiento
            : errorProcesamiento[..OutboxMensaje.LongitudMaximaErrorProcesamiento];

        var filasActualizadas = await MensajeReclamadoPor(contexto, idMensaje, tokenBloqueo)
            .ExecuteUpdateAsync(
                actualizador => actualizador
                    .SetProperty(mensaje => mensaje.Intentos, mensaje => mensaje.Intentos + 1)
                    .SetProperty(mensaje => mensaje.ErrorProcesamiento, errorNormalizado)
                    .SetProperty(mensaje => mensaje.BloqueadoHastaUtc, proximoIntentoUtc)
                    .SetProperty(mensaje => mensaje.TokenBloqueo, (Guid?)null),
                cancellationToken);

        return filasActualizadas == 1;
    }

    private static IQueryable<OutboxMensaje> MensajeReclamadoPor(
        RestauranteVentasDbContext contexto,
        Guid idMensaje,
        Guid tokenBloqueo) =>
        contexto.MensajesOutbox.Where(mensaje =>
            mensaje.Id == idMensaje &&
            mensaje.TokenBloqueo == tokenBloqueo &&
            mensaje.ProcesadoEnUtc == null);

    private static TimeSpan CalcularEsperaReintento(int intentosActuales)
    {
        var segundos = Math.Min(60, Math.Pow(2, Math.Min(intentosActuales + 1, 6)));
        return TimeSpan.FromSeconds(segundos);
    }
}
