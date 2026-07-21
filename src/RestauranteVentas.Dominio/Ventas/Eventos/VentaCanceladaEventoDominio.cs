using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Ventas.Eventos;

public sealed class VentaCanceladaEventoDominio : IEventoDominio
{
    public Guid VentaId { get; }
    public DateTime FechaUtc { get; }

    public VentaCanceladaEventoDominio(Guid ventaId, DateTime fechaUtc)
    {
        VentaId = ventaId;
        FechaUtc = fechaUtc;
    }
}
