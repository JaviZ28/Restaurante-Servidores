namespace RestauranteVentas.Dominio.Productos;

public interface IRepositorioProductoMenu
{
    ProductoMenu? ObtenerPorId(Guid id);

    void Agregar(ProductoMenu producto);
}
