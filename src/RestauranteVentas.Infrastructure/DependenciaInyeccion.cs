using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Productos.Queries;
using RestauranteVentas.Aplicacion.Ventas.Queries;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Infrastructure.Persistencia;
using RestauranteVentas.Infrastructure.Persistencia.Lecturas;
using RestauranteVentas.Infrastructure.Persistencia.Outbox;
using RestauranteVentas.Infrastructure.Persistencia.Repositorios;
using RestauranteVentas.Infrastructure.Servicios;

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

        // /health representa disponibilidad real: además de la aplicación,
        // verifica que PostgreSQL acepte conexiones. /alive sigue usando solo
        // el chequeo liviano registrado por ServiceDefaults.
        servicios.AddHealthChecks()
            .AddDbContextCheck<RestauranteVentasDbContext>("postgresql", tags: ["ready"]);

        servicios.AddScoped<IRepositorioVenta, RepositorioVenta>();
        servicios.AddScoped<IRepositorioProductoMenu, RepositorioProductoMenu>();
        servicios.AddScoped<IVentaLectura, LecturaVentaEfCore>();
        servicios.AddScoped<IProductoMenuLectura, LecturaProductoMenuEfCore>();
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        servicios.AddScoped<IReloj, RelojSistema>();
        servicios.AddScoped<IGeneradorIdentidad, GeneradorIdentidadGuid>();
        servicios.AddHostedService<ProcesadorOutbox>();

        return servicios;
    }
}
