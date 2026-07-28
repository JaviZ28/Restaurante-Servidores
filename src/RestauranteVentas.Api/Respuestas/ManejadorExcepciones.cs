using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Infrastructure.Persistencia;

namespace RestauranteVentas.Api.Respuestas;

/// <summary>
/// Evita que detalles de infraestructura se filtren como respuestas HTML o 500
/// ambiguos. Los conflictos de concurrencia son parte del contrato HTTP.
/// </summary>
public sealed class ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var esConflictoConcurrencia = exception is ConflictoConcurrenciaException or DbUpdateConcurrencyException;
        var estado = esConflictoConcurrencia
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status500InternalServerError;

        if (esConflictoConcurrencia)
        {
            logger.LogInformation(exception, "Conflicto de concurrencia al procesar {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogError(exception, "Error no controlado al procesar {Path}", httpContext.Request.Path);
        }

        await Results.Problem(
            statusCode: estado,
            title: esConflictoConcurrencia ? "Conflicto.Concurrencia" : "Error.Inesperado",
            detail: esConflictoConcurrencia
                ? "El recurso fue modificado por otra operación. Actualice su estado y vuelva a intentarlo."
                : "Ocurrió un error inesperado al procesar la solicitud.",
            type: $"https://httpstatuses.com/{estado}",
            extensions: new Dictionary<string, object?>
            {
                ["codigo"] = esConflictoConcurrencia ? "Conflicto.Concurrencia" : "Error.Inesperado",
                ["categoria"] = esConflictoConcurrencia ? "Conflicto" : "Inesperado"
            }).ExecuteAsync(httpContext);

        return true;
    }
}
