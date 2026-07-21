namespace RestauranteVentas.Dominio.Abstracciones;

public class Resultado
{
    public bool EsExito { get; }
    public Error? Error { get; }

    protected Resultado(bool esExito, Error? error)
    {
        EsExito = esExito;
        Error = error;
    }

    public static Resultado Exito() => new(true, null);

    public static Resultado Fallo(Error error) => new(false, error);
}
