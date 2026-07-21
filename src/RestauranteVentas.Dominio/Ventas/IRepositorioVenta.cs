namespace RestauranteVentas.Dominio.Ventas;

public interface IRepositorioVenta
{
    Task<Venta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AgregarAsync(Venta venta, CancellationToken cancellationToken = default);
}
