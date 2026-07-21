namespace RestauranteVentas.Aplicacion.Abstracciones;

public sealed class ErrorAplicacion
{
    public string Codigo { get; }
    public string Mensaje { get; }

    public ErrorAplicacion(string codigo, string mensaje)
    {
        Codigo = codigo;
        Mensaje = mensaje;
    }

    public static ErrorAplicacion DesdeDominio(string codigo, string mensaje) =>
        new(codigo, mensaje);
}
