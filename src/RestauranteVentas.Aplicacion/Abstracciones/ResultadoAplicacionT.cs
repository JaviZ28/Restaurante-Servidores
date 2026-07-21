namespace RestauranteVentas.Aplicacion.Abstracciones;

public sealed class ResultadoAplicacion<T> : ResultadoAplicacion
{
    public T? Valor { get; }

    private ResultadoAplicacion(T valor)
        : base(true, null) => Valor = valor;

    private ResultadoAplicacion(ErrorAplicacion error)
        : base(false, error) => Valor = default;

    public static ResultadoAplicacion<T> Exito(T valor) => new(valor);

    public new static ResultadoAplicacion<T> Fallo(string codigo, string mensaje) =>
        new(new ErrorAplicacion(codigo, mensaje));

    public new static ResultadoAplicacion<T> Fallo(ErrorAplicacion error) => new(error);
}
