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
        var estado = error.Tipo switch
        {
            TipoErrorAplicacion.NoEncontrado => StatusCodes.Status404NotFound,
            TipoErrorAplicacion.Conflicto => StatusCodes.Status409Conflict,
            TipoErrorAplicacion.ReglaNegocio => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            statusCode: estado,
            title: error.Codigo,
            detail: error.Mensaje,
            type: $"https://httpstatuses.com/{estado}",
            extensions: new Dictionary<string, object?>
            {
                ["codigo"] = error.Codigo,
                ["categoria"] = error.Tipo.ToString()
            });
    }
}
