using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Compartido;

public sealed class Cantidad : IEquatable<Cantidad>
{
    public const string CodigoInvalida = "Cantidad.Invalida";

    public int Valor { get; }

    private Cantidad(int valor) => Valor = valor;

    public static Resultado<Cantidad> Crear(int valor)
    {
        if (valor <= 0)
        {
            return Resultado<Cantidad>.Fallo(
                new Error(CodigoInvalida, "La cantidad debe ser mayor que cero."));
        }

        return Resultado<Cantidad>.Exito(new Cantidad(valor));
    }

    public bool Equals(Cantidad? other) => other is not null && Valor == other.Valor;

    public override bool Equals(object? obj) => obj is Cantidad other && Equals(other);

    public override int GetHashCode() => Valor.GetHashCode();
}
