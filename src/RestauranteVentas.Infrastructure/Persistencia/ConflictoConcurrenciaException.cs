namespace RestauranteVentas.Infrastructure.Persistencia;

/// <summary>
/// Señala que otro proceso modificó el agregado antes de confirmar la unidad
/// de trabajo actual. La API debe traducirla a HTTP 409 Conflict.
/// </summary>
public sealed class ConflictoConcurrenciaException : Exception
{
    public ConflictoConcurrenciaException(Exception innerException)
        : base(
            "No se pudieron guardar los cambios porque el recurso fue modificado por otra operación.",
            innerException)
    {
    }
}
