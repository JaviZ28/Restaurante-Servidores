using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Ventas.Eventos;

/// <summary>
/// Señala la apertura de una venta que, mientras permanece abierta, representa
/// la comanda en curso del restaurante.
/// </summary>
public sealed record VentaCreadaEventoDominio(
    Guid EventoId,
    Guid VentaId,
    DateTime FechaCreacionUtc) : IEventoDominio
{
    public DateTime OcurridoEnUtc => FechaCreacionUtc;

    // Se conserva como alias para consumidores ya existentes.
    public DateTime FechaUtc => OcurridoEnUtc;

    public VentaCreadaEventoDominio(Guid ventaId, DateTime fechaCreacionUtc)
        : this(Guid.NewGuid(), ventaId, fechaCreacionUtc)
    {
    }
}
