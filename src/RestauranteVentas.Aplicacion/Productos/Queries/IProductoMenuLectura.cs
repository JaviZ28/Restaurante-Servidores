using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Productos.Queries;

/// <summary>
/// Puerto de lectura de catálogo. Devuelve una proyección inmutable y nunca
/// expone el agregado de escritura a una consulta.
/// </summary>
public interface IProductoMenuLectura
{
    Task<IReadOnlyCollection<ProductoMenuDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    Task<ProductoMenuDto?> ObtenerPorIdAsync(Guid productoMenuId, CancellationToken cancellationToken = default);
}
