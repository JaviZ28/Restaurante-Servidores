using RestauranteVentas.Dominio.Abstracciones;
using RestauranteVentas.Dominio.Compartido;

namespace RestauranteVentas.Dominio.Ventas.Eventos;

/// <summary>
/// Señala que la comanda fue cerrada mediante un cobro definitivo.
/// </summary>
public sealed record VentaPagadaEventoDominio(
    Guid EventoId,
    Guid VentaId,
    Dinero Total,
    MetodoPago MetodoPago,
    DateTime FechaPagoUtc) : IEventoDominio
{
    public DateTime OcurridoEnUtc => FechaPagoUtc;

    // Se conserva como alias para consumidores ya existentes.
    public DateTime FechaUtc => OcurridoEnUtc;

    public VentaPagadaEventoDominio(
        Guid ventaId,
        Dinero total,
        MetodoPago metodoPago,
        DateTime fechaPagoUtc)
        : this(Guid.NewGuid(), ventaId, total, metodoPago, fechaPagoUtc)
    {
    }
}
