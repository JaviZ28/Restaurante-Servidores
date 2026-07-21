namespace RestauranteVentas.Aplicacion.Abstracciones;

public interface IUnidadDeTrabajo
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
