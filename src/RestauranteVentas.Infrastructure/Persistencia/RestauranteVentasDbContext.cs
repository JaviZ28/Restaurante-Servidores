using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Dominio.Abstracciones;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Infrastructure.Persistencia.Outbox;

namespace RestauranteVentas.Infrastructure.Persistencia;

public sealed class RestauranteVentasDbContext(DbContextOptions<RestauranteVentasDbContext> opciones)
    : DbContext(opciones)
{
    public DbSet<ProductoMenu> ProductosMenu => Set<ProductoMenu>();

    public DbSet<Venta> Ventas => Set<Venta>();

    public DbSet<OutboxMensaje> MensajesOutbox => Set<OutboxMensaje>();

    protected override void OnModelCreating(ModelBuilder constructorModelo)
    {
        constructorModelo.ApplyConfigurationsFromAssembly(typeof(RestauranteVentasDbContext).Assembly);
        base.OnModelCreating(constructorModelo);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var entidadesConEventos = PersistirEventosEnOutbox();

        try
        {
            var cambios = base.SaveChanges(acceptAllChangesOnSuccess);
            LimpiarEventos(entidadesConEventos);
            return cambios;
        }
        catch
        {
            // Los eventos permanecen en el agregado y los mensajes siguen
            // tracked. Esto permite reintentar sin perder eventos ni crear
            // mensajes duplicados dentro de la misma unidad de trabajo.
            throw;
        }
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var entidadesConEventos = PersistirEventosEnOutbox();

        try
        {
            var cambios = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            LimpiarEventos(entidadesConEventos);
            return cambios;
        }
        catch
        {
            // Ver comentario equivalente en la sobrecarga sincrónica.
            throw;
        }
    }

    private IReadOnlyCollection<Entidad> PersistirEventosEnOutbox()
    {
        ChangeTracker.DetectChanges();

        var entidadesConEventos = ChangeTracker
            .Entries<Entidad>()
            .Where(entrada => entrada.State is EntityState.Added or EntityState.Modified)
            .Select(entrada => entrada.Entity)
            .Where(entidad => entidad.Eventos.Count > 0)
            .Distinct()
            .ToArray();

        if (entidadesConEventos.Length == 0)
        {
            return entidadesConEventos;
        }

        var idsMensajesYaTrackeados = ChangeTracker
            .Entries<OutboxMensaje>()
            .Where(entrada => entrada.State != EntityState.Detached)
            .Select(entrada => entrada.Entity.Id)
            .ToHashSet();

        var creadoEnUtc = DateTime.UtcNow;
        var mensajesNuevos = entidadesConEventos
            .SelectMany(entidad => entidad.Eventos)
            .Where(evento => idsMensajesYaTrackeados.Add(evento.EventoId))
            .Select(evento => OutboxMensaje.Crear(evento, creadoEnUtc))
            .ToArray();

        if (mensajesNuevos.Length > 0)
        {
            MensajesOutbox.AddRange(mensajesNuevos);
        }

        return entidadesConEventos;
    }

    private static void LimpiarEventos(IEnumerable<Entidad> entidades)
    {
        foreach (var entidad in entidades)
        {
            entidad.LimpiarEventos();
        }
    }
}
