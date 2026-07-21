namespace RestauranteVentas.Dominio.Abstracciones;

public abstract class Entidad
{
    public Guid Id { get; private set; }

    private readonly List<IEventoDominio> _eventos = [];

    public IReadOnlyCollection<IEventoDominio> Eventos => _eventos.AsReadOnly();

    protected Entidad(Guid id) => Id = id;

    protected void RegistrarEvento(IEventoDominio evento) => _eventos.Add(evento);

    public void LimpiarEventos() => _eventos.Clear();
}
