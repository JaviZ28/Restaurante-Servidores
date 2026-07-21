namespace RestauranteVentas.Dominio.Abstracciones;

public sealed class Error
{
    public string Codigo { get; }
    public string Mensaje { get; }

    public Error(string codigo, string mensaje)
    {
        Codigo = codigo;
        Mensaje = mensaje;
    }
}
