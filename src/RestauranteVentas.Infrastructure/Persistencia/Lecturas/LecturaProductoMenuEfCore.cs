using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Productos.Queries;
using RestauranteVentas.Dominio.Compartido;

namespace RestauranteVentas.Infrastructure.Persistencia.Lecturas;

/// <summary>
/// Proyección de lectura del catálogo. No carga ni deja trackeado el agregado
/// de escritura.
/// </summary>
public sealed class LecturaProductoMenuEfCore(RestauranteVentasDbContext contexto) : IProductoMenuLectura
{
    public async Task<IReadOnlyCollection<ProductoMenuDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var proyecciones = await contexto.ProductosMenu
            .AsNoTracking()
            // Nombre usa un conversor de valor de EF Core. Ordenar mediante la
            // propiedad convertida permite traducirlo a la columna nombre;
            // acceder a Nombre.Valor aqui no puede traducirse a SQL.
            .OrderBy(producto => producto.Nombre)
            .ThenBy(producto => producto.Id)
            .Select(producto => new ProductoMenuProyeccion(
                producto.Id,
                producto.Nombre,
                producto.PrecioActual,
                producto.EstaActivo))
            .ToListAsync(cancellationToken);

        return proyecciones
            .Select(AProductoMenuDto)
            .ToList();
    }

    public async Task<ProductoMenuDto?> ObtenerPorIdAsync(
        Guid productoMenuId,
        CancellationToken cancellationToken = default)
    {
        var proyeccion = await contexto.ProductosMenu
            .AsNoTracking()
            .Where(producto => producto.Id == productoMenuId)
            .Select(producto => new ProductoMenuProyeccion(
                producto.Id,
                producto.Nombre,
                producto.PrecioActual,
                producto.EstaActivo))
            .SingleOrDefaultAsync(cancellationToken);

        return proyeccion is null ? null : AProductoMenuDto(proyeccion);
    }

    private static ProductoMenuDto AProductoMenuDto(ProductoMenuProyeccion proyeccion) =>
        new(
            proyeccion.Id,
            proyeccion.Nombre.Valor,
            proyeccion.PrecioActual.Monto,
            proyeccion.PrecioActual.Moneda,
            proyeccion.EstaActivo);

    private sealed record ProductoMenuProyeccion(
        Guid Id,
        NombreProducto Nombre,
        Dinero PrecioActual,
        bool EstaActivo);
}
