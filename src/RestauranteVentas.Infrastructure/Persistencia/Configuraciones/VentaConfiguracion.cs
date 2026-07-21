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
        constructor.ToTable("ventas");

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
