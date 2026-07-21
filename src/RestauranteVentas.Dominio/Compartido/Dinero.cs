using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Compartido;

public sealed class Dinero : IEquatable<Dinero>
{
    public const string CodigoMontoInvalido = "Dinero.MontoInvalido";
    public const string CodigoMonedaInvalida = "Dinero.MonedaInvalida";
    public const string CodigoMonedaDistinta = "Dinero.MonedaDistinta";
    public const string MonedaUsd = "USD";

    public decimal Monto { get; }
    public string Moneda { get; }

    private Dinero(decimal monto, string moneda)
    {
        Monto = monto;
        Moneda = moneda;
    }

    public static Resultado<Dinero> Crear(decimal monto, string moneda = MonedaUsd)
    {
        if (monto <= 0)
        {
            return Resultado<Dinero>.Fallo(
                new Error(CodigoMontoInvalido, "El monto debe ser mayor que cero."));
        }

        if (!string.Equals(moneda, MonedaUsd, StringComparison.OrdinalIgnoreCase))
        {
            return Resultado<Dinero>.Fallo(
                new Error(CodigoMonedaInvalida, "La moneda debe ser USD."));
        }

        return Resultado<Dinero>.Exito(new Dinero(monto, MonedaUsd));
    }

    public Resultado<Dinero> Sumar(Dinero otro)
    {
        if (!string.Equals(Moneda, otro.Moneda, StringComparison.OrdinalIgnoreCase))
        {
            return Resultado<Dinero>.Fallo(
                new Error(CodigoMonedaDistinta, "No se pueden sumar montos de monedas diferentes."));
        }

        return Crear(Monto + otro.Monto, Moneda);
    }

    public Resultado<Dinero> Multiplicar(Cantidad cantidad) =>
        Crear(Monto * cantidad.Valor, Moneda);

    public bool Equals(Dinero? other) =>
        other is not null && Monto == other.Monto &&
        string.Equals(Moneda, other.Moneda, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is Dinero other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Monto, Moneda.ToUpperInvariant());
}
