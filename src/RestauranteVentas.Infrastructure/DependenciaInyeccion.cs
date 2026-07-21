using Microsoft.Extensions.DependencyInjection;

namespace RestauranteVentas.Infrastructure;

public static class DependenciaInyeccion
{
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios) =>
        servicios;
}
