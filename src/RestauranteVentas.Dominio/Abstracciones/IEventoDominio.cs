namespace RestauranteVentas.Dominio.Abstracciones;

public interface IEventoDominio
{
    /// <summary>
    /// Identifica de forma estable el hecho de dominio para que los consumidores
    /// puedan procesarlo de forma idempotente.
    /// </summary>
    Guid EventoId { get; }

    /// <summary>
    /// Instante UTC en el que ocurrió el hecho de negocio.
    /// </summary>
    DateTime OcurridoEnUtc { get; }
}
