namespace RestauranteVentas.Aplicacion.Abstracciones;

public sealed class ErrorAplicacion
{
    public string Codigo { get; }
    public string Mensaje { get; }
    public TipoErrorAplicacion Tipo { get; }

    public ErrorAplicacion(
        string codigo,
        string mensaje,
        TipoErrorAplicacion tipo = TipoErrorAplicacion.Validacion)
    {
        Codigo = codigo;
        Mensaje = mensaje;
        Tipo = tipo;
    }

    public static ErrorAplicacion DesdeDominio(string codigo, string mensaje) =>
        new(codigo, mensaje, TipoErrorAplicacion.ReglaNegocio);
}
