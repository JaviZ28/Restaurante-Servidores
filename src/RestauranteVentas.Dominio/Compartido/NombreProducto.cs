using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Compartido;

public sealed class NombreProducto : IEquatable<NombreProducto>
{
    public const string CodigoVacio = "NombreProducto.Vacio";
    public const string CodigoLongitudInvalida = "NombreProducto.LongitudInvalida";
    public const int LongitudMaxima = 100;

    public string Valor { get; }

    private NombreProducto(string valor) => Valor = valor;

    public static Resultado<NombreProducto> Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return Resultado<NombreProducto>.Fallo(
                new Error(CodigoVacio, "El nombre del producto no puede estar vacío."));
        }

        var nombreNormalizado = valor.Trim();

        if (nombreNormalizado.Length > LongitudMaxima)
        {
            return Resultado<NombreProducto>.Fallo(
                new Error(CodigoLongitudInvalida, $"El nombre del producto no puede superar {LongitudMaxima} caracteres."));
        }

        return Resultado<NombreProducto>.Exito(new NombreProducto(nombreNormalizado));
    }

    public bool Equals(NombreProducto? other) =>
        other is not null && string.Equals(Valor, other.Valor, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is NombreProducto other && Equals(other);

    public override int GetHashCode() => Valor.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Valor;
}
