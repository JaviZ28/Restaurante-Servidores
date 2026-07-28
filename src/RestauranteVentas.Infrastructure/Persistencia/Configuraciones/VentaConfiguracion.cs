using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Infrastructure.Persistencia.Configuraciones;

public sealed class VentaConfiguracion : IEntityTypeConfiguration<Venta>
{
    private static readonly ValueConverter<NumeroMesa?, int?> ConvertidorMesa = new(
        mesa => mesa == null ? null : mesa.Valor,
        numeroMesa => numeroMesa.HasValue
            ? NumeroMesa.Crear(numeroMesa.Value).Valor
            : null);

    public void Configure(EntityTypeBuilder<Venta> constructor)
    {
        constructor.ToTable("ventas", tabla =>
        {
            tabla.HasCheckConstraint(
                "CK_ventas_cliente_id_no_vacio",
                "\"cliente_id\" IS NULL OR \"cliente_id\" <> '00000000-0000-0000-0000-000000000000'::uuid");
            tabla.HasCheckConstraint(
                "CK_ventas_numero_mesa_positivo",
                "\"numero_mesa\" IS NULL OR \"numero_mesa\" > 0");
            tabla.HasCheckConstraint(
                "CK_ventas_estado_valido",
                "\"estado\" IN ('Abierta', 'Pagada', 'Cancelada')");
            tabla.HasCheckConstraint(
                "CK_ventas_fecha_creacion_valida",
                "\"fecha_creacion_utc\" > TIMESTAMPTZ '0001-01-01 00:00:00+00'");
            tabla.HasCheckConstraint(
                "CK_ventas_fecha_pago_posterior",
                "\"fecha_pago_utc\" IS NULL OR \"fecha_pago_utc\" > \"fecha_creacion_utc\"");
            tabla.HasCheckConstraint(
                "CK_ventas_fecha_cancelacion_posterior",
                "\"fecha_cancelacion_utc\" IS NULL OR \"fecha_cancelacion_utc\" > \"fecha_creacion_utc\"");
            tabla.HasCheckConstraint(
                "CK_ventas_transicion_estado_coherente",
                "(\"estado\" = 'Abierta' AND \"fecha_pago_utc\" IS NULL AND \"metodo_pago\" IS NULL AND \"fecha_cancelacion_utc\" IS NULL AND \"motivo_cancelacion\" IS NULL) OR " +
                "(\"estado\" = 'Pagada' AND \"fecha_pago_utc\" IS NOT NULL AND \"metodo_pago\" IS NOT NULL AND \"fecha_cancelacion_utc\" IS NULL AND \"motivo_cancelacion\" IS NULL) OR " +
                "(\"estado\" = 'Cancelada' AND \"fecha_pago_utc\" IS NULL AND \"metodo_pago\" IS NULL AND \"fecha_cancelacion_utc\" IS NOT NULL AND length(btrim(coalesce(\"motivo_cancelacion\", ''))) > 0)");
            tabla.HasCheckConstraint(
                "CK_ventas_metodo_pago_valido",
                "\"metodo_pago\" IS NULL OR \"metodo_pago\" IN ('Efectivo', 'Tarjeta', 'Transferencia')");
        });

        constructor.HasKey(venta => venta.Id);

        constructor.Property(venta => venta.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        constructor.Property(venta => venta.ClienteId)
            .HasColumnName("cliente_id");

        constructor.Property(venta => venta.Mesa)
            .HasColumnName("numero_mesa")
            .HasConversion(ConvertidorMesa);

        constructor.Property(venta => venta.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(venta => venta.FechaCreacionUtc)
            .HasColumnName("fecha_creacion_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.Property(venta => venta.FechaPagoUtc)
            .HasColumnName("fecha_pago_utc")
            .HasColumnType("timestamp with time zone");

        constructor.Property(venta => venta.MetodoPago)
            .HasColumnName("metodo_pago")
            .HasConversion<string>()
            .HasMaxLength(20);

        constructor.Property(venta => venta.FechaCancelacionUtc)
            .HasColumnName("fecha_cancelacion_utc")
            .HasColumnType("timestamp with time zone");

        constructor.Property(venta => venta.MotivoCancelacion)
            .HasColumnName("motivo_cancelacion")
            .HasMaxLength(Venta.LongitudMaximaMotivoCancelacion);

        constructor.Property(venta => venta.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken()
            .ValueGeneratedNever()
            .IsRequired();

        // PostgreSQL incrementa la columna de sistema xmin con cada UPDATE.
        // EF la incluye en el WHERE de UPDATE/DELETE para detectar escrituras
        // concurrentes. Revision hace que también las mutaciones de detalles
        // emitan un UPDATE sobre la raíz del agregado.
        constructor.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        constructor.HasIndex(venta => new { venta.Estado, venta.FechaCreacionUtc })
            .HasDatabaseName("IX_ventas_estado_fecha_creacion_utc");

        constructor.HasMany(venta => venta.Detalles)
            .WithOne()
            .HasForeignKey("venta_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        constructor.Navigation(venta => venta.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        constructor.Ignore(venta => venta.Total);
        constructor.Ignore(venta => venta.Eventos);
    }
}
