namespace RestauranteVentas.Aplicacion.Abstracciones;

public interface IComandoHandler<in TComando, TResult>
    where TComando : IComando<TResult>
{
    Task<TResult> ManejarAsync(TComando comando, CancellationToken cancellationToken = default);
}
