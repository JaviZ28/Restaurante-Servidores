using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RestauranteVentas.Infrastructure.Persistencia;

public sealed class FabricaDbContextTiempoDisenio
    : IDesignTimeDbContextFactory<RestauranteVentasDbContext>
{
    private const string NombreVariableConexion = "RESTAURANTEVENTAS_CONNECTION_STRING";

    public RestauranteVentasDbContext CreateDbContext(string[] args)
    {
        var cadenaConexion = Environment.GetEnvironmentVariable(NombreVariableConexion);

        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                $"Defina la variable {NombreVariableConexion} para ejecutar las herramientas de Entity Framework Core.");
        }

        var opciones = new DbContextOptionsBuilder<RestauranteVentasDbContext>()
            .UseNpgsql(cadenaConexion)
            .Options;

        return new RestauranteVentasDbContext(opciones);
    }
}
