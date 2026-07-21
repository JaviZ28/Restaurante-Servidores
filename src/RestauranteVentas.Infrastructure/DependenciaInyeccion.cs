using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Infrastructure.Persistencia;
using RestauranteVentas.Infrastructure.Persistencia.Repositorios;

namespace RestauranteVentas.Infrastructure;

public static class DependenciaInyeccion
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios,
        string cadenaConexion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);

        servicios.AddDbContext<RestauranteVentasDbContext>(opciones =>
            opciones.UseNpgsql(cadenaConexion));

        servicios.AddScoped<IRepositorioVenta, RepositorioVenta>();
        servicios.AddScoped<IRepositorioProductoMenu, RepositorioProductoMenu>();
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        return servicios;
    }
}
