using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Infrastructure.Persistencia.Configuraciones;

public sealed class ProductoMenuConfiguracion : IEntityTypeConfiguration<ProductoMenu>
{
    public void Configure(EntityTypeBuilder<ProductoMenu> constructor)
    {
        constructor.ToTable("productos_menu", tabla =>
        {
            tabla.HasCheckConstraint(
                "CK_productos_menu_nombre_no_vacio",
                "length(btrim(\"nombre\")) > 0");
            tabla.HasCheckConstraint(
                "CK_productos_menu_precio_positivo",
                "\"precio_monto\" > 0");
        });

        constructor.HasKey(producto => producto.Id);

        constructor.Property(producto => producto.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        constructor.Property(producto => producto.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(NombreProducto.LongitudMaxima)
            .HasConversion(
                nombre => nombre.Valor,
                valor => NombreProducto.Crear(valor).Valor!);

        constructor.Property(producto => producto.PrecioActual)
            .HasColumnName("precio_monto")
            .HasConversion(
                dinero => dinero.Monto,
                monto => Dinero.Crear(monto).Valor!)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(producto => producto.EstaActivo)
            .HasColumnName("esta_activo")
            .IsRequired();

        constructor.HasIndex(producto => new { producto.EstaActivo, producto.Nombre })
            .HasDatabaseName("IX_productos_menu_activo_nombre");

        constructor.Ignore(producto => producto.Eventos);
    }
}
