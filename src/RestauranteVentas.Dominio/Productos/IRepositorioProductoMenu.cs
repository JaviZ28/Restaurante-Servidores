namespace RestauranteVentas.Dominio.Productos;

public interface IRepositorioProductoMenu
{
    Task<ProductoMenu?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarAsync(ProductoMenu producto, CancellationToken cancellationToken = default);
}
