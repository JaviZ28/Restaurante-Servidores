using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Infrastructure.Persistencia;

public sealed class RestauranteVentasDbContext(DbContextOptions<RestauranteVentasDbContext> opciones)
    : DbContext(opciones)
{
    public DbSet<ProductoMenu> ProductosMenu => Set<ProductoMenu>();

    public DbSet<Venta> Ventas => Set<Venta>();

    protected override void OnModelCreating(ModelBuilder constructorModelo)
    {
        constructorModelo.ApplyConfigurationsFromAssembly(typeof(RestauranteVentasDbContext).Assembly);
        base.OnModelCreating(constructorModelo);
    }
}
