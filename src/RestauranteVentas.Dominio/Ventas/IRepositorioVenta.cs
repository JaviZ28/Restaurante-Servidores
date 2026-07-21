namespace RestauranteVentas.Dominio.Ventas;

public interface IRepositorioVenta
{
    Venta? ObtenerPorId(Guid id);

    void Agregar(Venta venta);
}
