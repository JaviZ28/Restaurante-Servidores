using RestauranteVentas.Dominio.Abstracciones;

namespace RestauranteVentas.Dominio.Tests.Abstracciones;

public class EntidadTests
{
    [Fact]
    public void Registrar_evento_rechaza_nulo()
    {
        var entidad = new EntidadPrueba();

        Assert.Throws<ArgumentNullException>(() => entidad.Registrar(null!));
        Assert.Empty(entidad.Eventos);
    }

    [Fact]
    public void Eventos_no_pueden_modificarse_desde_fuera_del_agregado()
    {
        var entidad = new EntidadPrueba();
        entidad.Registrar(CrearEvento());

        var eventosExpuestos = Assert.IsAssignableFrom<ICollection<IEventoDominio>>(entidad.Eventos);

        Assert.Throws<NotSupportedException>(() => eventosExpuestos.Add(CrearEvento()));
        Assert.Single(entidad.Eventos);
    }

    [Fact]
    public void Extraer_eventos_devuelve_instantanea_y_limpia_el_agregado()
    {
        var entidad = new EntidadPrueba();
        var evento = CrearEvento();
        entidad.Registrar(evento);

        var extraidos = entidad.ExtraerEventos();

        Assert.Single(extraidos);
        Assert.Same(evento, extraidos.Single());
        Assert.Empty(entidad.Eventos);

        var eventosExtraidos = Assert.IsAssignableFrom<ICollection<IEventoDominio>>(extraidos);
        Assert.Throws<NotSupportedException>(() => eventosExtraidos.Add(CrearEvento()));
    }

    private static EventoPrueba CrearEvento() => new(Guid.NewGuid(), DateTime.UtcNow);

    private sealed class EntidadPrueba : Entidad
    {
        public EntidadPrueba()
            : base(Guid.NewGuid())
        {
        }

        public void Registrar(IEventoDominio evento) => RegistrarEvento(evento);
    }

    private sealed record EventoPrueba(Guid EventoId, DateTime OcurridoEnUtc) : IEventoDominio;
}
