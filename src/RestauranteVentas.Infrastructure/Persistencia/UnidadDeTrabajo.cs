using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Aplicacion.Abstracciones;

namespace RestauranteVentas.Infrastructure.Persistencia;

public sealed class UnidadDeTrabajo(RestauranteVentasDbContext contexto) : IUnidadDeTrabajo
{
    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException excepcion)
        {
            throw new ConflictoConcurrenciaException(excepcion);
        }
    }
}
