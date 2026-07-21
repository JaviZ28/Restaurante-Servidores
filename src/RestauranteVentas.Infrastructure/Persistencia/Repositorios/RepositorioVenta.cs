using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioVenta(RestauranteVentasDbContext contexto) : IRepositorioVenta
{
    public Task<Venta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Ventas
            .Include(venta => venta.Detalles)
            .SingleOrDefaultAsync(venta => venta.Id == id, cancellationToken);

    public async Task AgregarAsync(Venta venta, CancellationToken cancellationToken = default) =>
        await contexto.Ventas.AddAsync(venta, cancellationToken);
}
