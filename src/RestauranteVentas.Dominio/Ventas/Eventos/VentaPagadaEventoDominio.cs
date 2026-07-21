using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Ventas.Eventos;

public sealed class VentaPagadaEventoDominio : IEventoDominio
{
    public Guid VentaId { get; }
    public DateTime FechaUtc { get; }

    public VentaPagadaEventoDominio(Guid ventaId, DateTime fechaUtc)
    {
        VentaId = ventaId;
        FechaUtc = fechaUtc;
    }
}
