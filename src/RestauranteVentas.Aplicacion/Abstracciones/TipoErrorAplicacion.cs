namespace RestauranteVentas.Aplicacion.Abstracciones;

/// <summary>
/// Clasifica un fallo de aplicación sin acoplar los casos de uso a HTTP.
/// La capa API traduce esta clasificación a un código de respuesta apropiado.
/// </summary>
public enum TipoErrorAplicacion
{
    Validacion,
    NoEncontrado,
    Conflicto,
    ReglaNegocio
}
