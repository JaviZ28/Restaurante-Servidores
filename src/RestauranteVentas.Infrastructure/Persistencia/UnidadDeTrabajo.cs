using RestauranteVentas.Aplicacion.Abstracciones;

namespace RestauranteVentas.Infrastructure.Persistencia;

public sealed class UnidadDeTrabajo(RestauranteVentasDbContext contexto) : IUnidadDeTrabajo
{
    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        await contexto.SaveChangesAsync(cancellationToken);
}
