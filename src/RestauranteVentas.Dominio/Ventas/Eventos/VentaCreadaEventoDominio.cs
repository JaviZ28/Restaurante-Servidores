using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Ventas.Eventos;

public sealed class VentaCreadaEventoDominio : IEventoDominio
{
    public Guid VentaId { get; }
    public DateTime FechaUtc { get; }

    public VentaCreadaEventoDominio(Guid ventaId, DateTime fechaUtc)
    {
        VentaId = ventaId;
        FechaUtc = fechaUtc;
    }
}
