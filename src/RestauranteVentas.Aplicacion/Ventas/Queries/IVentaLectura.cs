using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Queries;

/// <summary>
/// Puerto CQRS para proyecciones de ventas. La implementación debe consultar
/// sin tracking y no reconstruir agregados solo para responder una lectura.
/// </summary>
public interface IVentaLectura
{
    Task<IReadOnlyCollection<VentaDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);

    Task<VentaDto?> ObtenerPorIdAsync(Guid ventaId, CancellationToken cancellationToken = default);
}
