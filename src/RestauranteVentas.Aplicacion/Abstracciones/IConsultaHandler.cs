namespace RestauranteVentas.Aplicacion.Abstracciones;

public interface IConsultaHandler<in TConsulta, TResult>
    where TConsulta : IConsulta<TResult>
{
    Task<TResult> ManejarAsync(TConsulta consulta, CancellationToken cancellationToken = default);
}
