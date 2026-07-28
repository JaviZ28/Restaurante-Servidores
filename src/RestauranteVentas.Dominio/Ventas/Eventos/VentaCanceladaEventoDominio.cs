using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Ventas.Eventos;

/// <summary>
/// Señala que una comanda abierta fue cancelada conservando su motivo de
/// auditoría.
/// </summary>
public sealed record VentaCanceladaEventoDominio(
    Guid EventoId,
    Guid VentaId,
    string MotivoCancelacion,
    DateTime FechaCancelacionUtc) : IEventoDominio
{
    public DateTime OcurridoEnUtc => FechaCancelacionUtc;

    // Se conserva como alias para consumidores ya existentes.
    public DateTime FechaUtc => OcurridoEnUtc;

    public VentaCanceladaEventoDominio(
        Guid ventaId,
        string motivoCancelacion,
        DateTime fechaCancelacionUtc)
        : this(Guid.NewGuid(), ventaId, motivoCancelacion, fechaCancelacionUtc)
    {
    }
}
