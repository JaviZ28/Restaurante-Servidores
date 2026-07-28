namespace RestauranteVentas.Dominio.Abstracciones;

public abstract class Entidad
{
    public Guid Id { get; private set; }

    private readonly List<IEventoDominio> _eventos = [];

    /// <summary>
    /// Eventos aún pendientes de persistirse o despacharse. La colección es de
    /// solo lectura para impedir que consumidores externos alteren el agregado.
    /// </summary>
    public IReadOnlyCollection<IEventoDominio> Eventos => _eventos.AsReadOnly();

    protected Entidad(Guid id) => Id = id;

    protected void RegistrarEvento(IEventoDominio evento)
    {
        ArgumentNullException.ThrowIfNull(evento);
        _eventos.Add(evento);
    }

    /// <summary>
    /// Obtiene una instantánea inmutable de los eventos pendientes y los retira
    /// del agregado en una sola operación. Es útil para adaptadores que los
    /// transfieren a una cola propia; para un outbox transaccional conviene usar
    /// <see cref="Eventos"/> y limpiar únicamente después de guardar con éxito.
    /// </summary>
    public IReadOnlyCollection<IEventoDominio> ExtraerEventos()
    {
        if (_eventos.Count == 0)
        {
            return Array.Empty<IEventoDominio>();
        }

        var eventos = _eventos.ToArray();
        _eventos.Clear();
        return Array.AsReadOnly(eventos);
    }

    /// <summary>
    /// Marca como despachados los eventos pendientes. Debe invocarse solamente
    /// después de que su persistencia transaccional haya sido exitosa.
    /// </summary>
    public void LimpiarEventos() => _eventos.Clear();
}
