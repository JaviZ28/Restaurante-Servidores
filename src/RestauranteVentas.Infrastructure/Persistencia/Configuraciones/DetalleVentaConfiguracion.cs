using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Infrastructure.Persistencia.Configuraciones;

public sealed class DetalleVentaConfiguracion : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> constructor)
    {
        constructor.ToTable("detalles_venta");

        constructor.HasKey(detalle => detalle.Id);

        constructor.Property(detalle => detalle.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        constructor.Property(detalle => detalle.ProductoMenuId)
            .HasColumnName("producto_menu_id")
            .IsRequired();

        constructor.HasIndex(detalle => detalle.ProductoMenuId);

        constructor.Property(detalle => detalle.NombreHistorico)
            .HasColumnName("nombre_historico")
            .HasMaxLength(NombreProducto.LongitudMaxima)
            .HasConversion(
                nombre => nombre.Valor,
                valor => NombreProducto.Crear(valor).Valor!);

        constructor.Property(detalle => detalle.PrecioUnitarioHistorico)
            .HasColumnName("precio_unitario_monto")
            .HasConversion(
                dinero => dinero.Monto,
                monto => Dinero.Crear(monto).Valor!)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(detalle => detalle.Cantidad)
            .HasColumnName("cantidad")
            .HasConversion(
                cantidad => cantidad.Valor,
                valor => Cantidad.Crear(valor).Valor!)
            .IsRequired();

        constructor.Ignore(detalle => detalle.Subtotal);
    }
}
