using System.Text.Json;
using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Infrastructure.Persistencia.Outbox;

/// <summary>
/// Representa un evento de dominio persistido dentro de la misma transacción
/// que modifica el agregado. Un publicador posterior puede entregar estos
/// mensajes de manera confiable sin acoplar el dominio a infraestructura.
/// </summary>
public sealed class OutboxMensaje
{
    public const int LongitudMaximaErrorProcesamiento = 4_000;

    private static readonly JsonSerializerOptions OpcionesSerializacion = new(JsonSerializerDefaults.Web);

    private OutboxMensaje()
    {
    }

    private OutboxMensaje(
        Guid id,
        string tipoEvento,
        string contenido,
        DateTime ocurridoEnUtc,
        DateTime creadoEnUtc)
    {
        Id = id;
        TipoEvento = tipoEvento;
        Contenido = contenido;
        OcurridoEnUtc = ocurridoEnUtc;
        CreadoEnUtc = creadoEnUtc;
    }

    /// <summary>
    /// Coincide con <see cref="IEventoDominio.EventoId"/> y evita insertar el
    /// mismo evento dos veces al reintentar una unidad de trabajo.
    /// </summary>
    public Guid Id { get; private set; }

    public string TipoEvento { get; private set; } = string.Empty;

    public string Contenido { get; private set; } = string.Empty;

    public DateTime OcurridoEnUtc { get; private set; }

    public DateTime CreadoEnUtc { get; private set; }

    public DateTime? ProcesadoEnUtc { get; private set; }

    /// <summary>
    /// Reserva temporal para impedir que dos instancias publiquen el mismo
    /// mensaje al mismo tiempo. Si una instancia termina inesperadamente, el
    /// mensaje vuelve a estar disponible al expirar la reserva.
    /// </summary>
    public DateTime? BloqueadoHastaUtc { get; private set; }

    public Guid? TokenBloqueo { get; private set; }

    public int Intentos { get; private set; }

    public string? ErrorProcesamiento { get; private set; }

    public static OutboxMensaje Crear(IEventoDominio evento, DateTime creadoEnUtc)
    {
        ArgumentNullException.ThrowIfNull(evento);

        if (evento.EventoId == Guid.Empty)
        {
            throw new ArgumentException("Un evento de dominio debe tener un identificador no vacío.", nameof(evento));
        }

        if (evento.OcurridoEnUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha de ocurrencia del evento debe estar en UTC.", nameof(evento));
        }

        if (creadoEnUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha de creación del mensaje debe estar en UTC.", nameof(creadoEnUtc));
        }

        var tipoEvento = evento.GetType().FullName ?? evento.GetType().Name;
        var contenido = JsonSerializer.Serialize(evento, evento.GetType(), OpcionesSerializacion);

        return new OutboxMensaje(
            evento.EventoId,
            tipoEvento,
            contenido,
            evento.OcurridoEnUtc,
            creadoEnUtc);
    }

    public void MarcarComoProcesado(DateTime procesadoEnUtc)
    {
        ValidarFechaUtc(procesadoEnUtc, nameof(procesadoEnUtc));

        ProcesadoEnUtc = procesadoEnUtc;
        BloqueadoHastaUtc = null;
        TokenBloqueo = null;
        ErrorProcesamiento = null;
    }

    public void RegistrarFallo(string errorProcesamiento, DateTime proximoIntentoUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorProcesamiento);
        ValidarFechaUtc(proximoIntentoUtc, nameof(proximoIntentoUtc));

        Intentos++;
        ErrorProcesamiento = errorProcesamiento.Length <= LongitudMaximaErrorProcesamiento
            ? errorProcesamiento
            : errorProcesamiento[..LongitudMaximaErrorProcesamiento];
        BloqueadoHastaUtc = proximoIntentoUtc;
        TokenBloqueo = null;
    }

    private static void ValidarFechaUtc(DateTime fechaUtc, string nombreParametro)
    {
        if (fechaUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha debe estar en UTC.", nombreParametro);
        }
    }
}
