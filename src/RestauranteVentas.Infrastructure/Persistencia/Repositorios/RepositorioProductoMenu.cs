using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioProductoMenu(RestauranteVentasDbContext contexto) : IRepositorioProductoMenu
{
    public Task<ProductoMenu?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.ProductosMenu
            .SingleOrDefaultAsync(producto => producto.Id == id, cancellationToken);

    public async Task AgregarAsync(ProductoMenu producto, CancellationToken cancellationToken = default) =>
        await contexto.ProductosMenu.AddAsync(producto, cancellationToken);
}
