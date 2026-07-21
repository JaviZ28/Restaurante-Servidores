using RestauranteVentas.Aplicacion.Abstracciones;

namespace RestauranteVentas.Api.Respuestas;

public static class ResultadosHttp
{
    public static IResult Desde<T>(ResultadoAplicacion<T> resultado, string? ubicacion = null)
    {
        if (resultado.EsExito)
        {
            return ubicacion is null
                ? Results.Ok(resultado.Valor)
                : Results.Created(ubicacion, resultado.Valor);
        }

        var error = resultado.Error!;
        var estado = error.Codigo.EndsWith("NoEncontrada", StringComparison.Ordinal) ||
                     error.Codigo.EndsWith("NoEncontrado", StringComparison.Ordinal)
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return Results.Problem(
            statusCode: estado,
            title: error.Codigo,
            detail: error.Mensaje);
    }
}
