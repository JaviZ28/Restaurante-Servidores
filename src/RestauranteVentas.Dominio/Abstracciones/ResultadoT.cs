namespace RestauranteVentas.Dominio.Abstracciones;

public sealed class Resultado<T> : Resultado
{
    public T? Valor { get; }

    private Resultado(T valor)
        : base(true, null) => Valor = valor;

    private Resultado(Error error)
        : base(false, error) => Valor = default;

    public static Resultado<T> Exito(T valor) => new(valor);

    public new static Resultado<T> Fallo(Error error) => new(error);
}
