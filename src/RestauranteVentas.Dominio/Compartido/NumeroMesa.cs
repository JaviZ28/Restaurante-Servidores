using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Compartido;

public sealed class NumeroMesa : IEquatable<NumeroMesa>
{
    public const string CodigoInvalido = "NumeroMesa.Invalido";

    public int Valor { get; }

    private NumeroMesa(int valor) => Valor = valor;

    public static Resultado<NumeroMesa> Crear(int valor)
    {
        if (valor <= 0)
        {
            return Resultado<NumeroMesa>.Fallo(
                new Error(CodigoInvalido, "El número de mesa debe ser mayor que cero."));
        }

        return Resultado<NumeroMesa>.Exito(new NumeroMesa(valor));
    }

    public bool Equals(NumeroMesa? other) => other is not null && Valor == other.Valor;

    public override bool Equals(object? obj) => obj is NumeroMesa other && Equals(other);

    public override int GetHashCode() => Valor.GetHashCode();
}
