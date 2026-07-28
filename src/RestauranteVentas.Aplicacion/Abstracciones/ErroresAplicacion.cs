using RestauranteVentas.Dominio.Abstracciones;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Abstracciones;

/// <summary>
/// Punto único para preservar la semántica de los errores de dominio al salir
/// de un caso de uso. Application conoce reglas, pero no conoce HTTP.
/// </summary>
public static class ErroresAplicacion
{
    private static readonly IReadOnlyDictionary<string, TipoErrorAplicacion> TiposPorCodigo =
        new Dictionary<string, TipoErrorAplicacion>
        {
            [Cantidad.CodigoInvalida] = TipoErrorAplicacion.Validacion,
            [Dinero.CodigoMontoInvalido] = TipoErrorAplicacion.Validacion,
            [Dinero.CodigoMonedaInvalida] = TipoErrorAplicacion.Validacion,
            [NombreProducto.CodigoVacio] = TipoErrorAplicacion.Validacion,
            [NombreProducto.CodigoLongitudInvalida] = TipoErrorAplicacion.Validacion,
            [NumeroMesa.CodigoInvalido] = TipoErrorAplicacion.Validacion,
            [ErroresProductoMenu.IdInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresProductoMenu.NombreInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresProductoMenu.PrecioInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.IdInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.ClienteIdInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.FechaCreacionInvalida.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.ProductoInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.CantidadInvalida.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.MetodoPagoInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.FechaPagoInvalida.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.FechaPagoNoPosteriorACreacion.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.FechaCancelacionInvalida.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.FechaCancelacionNoPosteriorACreacion.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.MotivoCancelacionInvalido.Codigo] = TipoErrorAplicacion.Validacion,
            [ErroresVenta.DetalleNoEncontrado.Codigo] = TipoErrorAplicacion.NoEncontrado,
            [ErroresVenta.YaPagada.Codigo] = TipoErrorAplicacion.Conflicto,
            [ErroresVenta.YaCancelada.Codigo] = TipoErrorAplicacion.Conflicto,
            [ErroresVenta.VentaNoAbierta.Codigo] = TipoErrorAplicacion.Conflicto
        };

    public static ErrorAplicacion Validacion(string codigo, string mensaje) =>
        new(codigo, mensaje, TipoErrorAplicacion.Validacion);

    public static ErrorAplicacion NoEncontrado(string codigo, string mensaje) =>
        new(codigo, mensaje, TipoErrorAplicacion.NoEncontrado);

    public static ErrorAplicacion Conflicto(string codigo, string mensaje) =>
        new(codigo, mensaje, TipoErrorAplicacion.Conflicto);

    public static ErrorAplicacion ReglaNegocio(string codigo, string mensaje) =>
        new(codigo, mensaje, TipoErrorAplicacion.ReglaNegocio);

    public static ErrorAplicacion DesdeDominio(Error error)
    {
        var tipo = TiposPorCodigo.TryGetValue(error.Codigo, out var tipoMapeado)
            ? tipoMapeado
            : TipoErrorAplicacion.ReglaNegocio;

        return new ErrorAplicacion(error.Codigo, error.Mensaje, tipo);
    }
}
