using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteVentas.Infrastructure.Persistencia.Outbox;

namespace RestauranteVentas.Infrastructure.Persistencia.Configuraciones;

public sealed class OutboxMensajeConfiguracion : IEntityTypeConfiguration<OutboxMensaje>
{
    public void Configure(EntityTypeBuilder<OutboxMensaje> constructor)
    {
        constructor.ToTable("outbox_mensajes", tabla =>
        {
            tabla.HasCheckConstraint(
                "CK_outbox_mensajes_intentos_no_negativos",
                "\"intentos\" >= 0");
        });

        constructor.HasKey(mensaje => mensaje.Id);

        constructor.Property(mensaje => mensaje.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        constructor.Property(mensaje => mensaje.TipoEvento)
            .HasColumnName("tipo_evento")
            .HasMaxLength(500)
            .IsRequired();

        constructor.Property(mensaje => mensaje.Contenido)
            .HasColumnName("contenido")
            .HasColumnType("jsonb")
            .IsRequired();

        constructor.Property(mensaje => mensaje.OcurridoEnUtc)
            .HasColumnName("ocurrido_en_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.Property(mensaje => mensaje.CreadoEnUtc)
            .HasColumnName("creado_en_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.Property(mensaje => mensaje.ProcesadoEnUtc)
            .HasColumnName("procesado_en_utc")
            .HasColumnType("timestamp with time zone");

        constructor.Property(mensaje => mensaje.BloqueadoHastaUtc)
            .HasColumnName("bloqueado_hasta_utc")
            .HasColumnType("timestamp with time zone");

        constructor.Property(mensaje => mensaje.TokenBloqueo)
            .HasColumnName("token_bloqueo");

        constructor.Property(mensaje => mensaje.Intentos)
            .HasColumnName("intentos")
            .IsRequired();

        constructor.Property(mensaje => mensaje.ErrorProcesamiento)
            .HasColumnName("error_procesamiento")
            .HasMaxLength(OutboxMensaje.LongitudMaximaErrorProcesamiento);

        constructor.HasIndex(mensaje => new
            { mensaje.ProcesadoEnUtc, mensaje.BloqueadoHastaUtc, mensaje.OcurridoEnUtc })
            .HasDatabaseName("IX_outbox_mensajes_pendientes_ocurrido");
    }
}
