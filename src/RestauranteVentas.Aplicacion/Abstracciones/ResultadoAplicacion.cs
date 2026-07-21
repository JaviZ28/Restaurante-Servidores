namespace RestauranteVentas.Aplicacion.Abstracciones;

public class ResultadoAplicacion
{
    public bool EsExito { get; }
    public ErrorAplicacion? Error { get; }

    protected ResultadoAplicacion(bool esExito, ErrorAplicacion? error)
    {
        EsExito = esExito;
        Error = error;
    }

    public static ResultadoAplicacion Exito() => new(true, null);

    public static ResultadoAplicacion Fallo(string codigo, string mensaje) =>
        new(false, new ErrorAplicacion(codigo, mensaje));

    public static ResultadoAplicacion Fallo(ErrorAplicacion error) => new(false, error);
}
