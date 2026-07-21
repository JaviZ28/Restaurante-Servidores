using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Infrastructure.Persistencia.Configuraciones;

public sealed class ProductoMenuConfiguracion : IEntityTypeConfiguration<ProductoMenu>
{
    public void Configure(EntityTypeBuilder<ProductoMenu> constructor)
    {
        constructor.ToTable("productos_menu");

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

        constructor.OwnsOne(producto => producto.PrecioActual, precio =>
        {
            precio.Property(dinero => dinero.Monto)
                .HasColumnName("precio_monto")
                .HasPrecision(18, 2)
                .IsRequired();

            precio.Property(dinero => dinero.Moneda)
                .HasColumnName("precio_moneda")
                .HasMaxLength(3)
                .IsRequired();
        });

        constructor.Property(producto => producto.EstaActivo)
            .HasColumnName("esta_activo")
            .IsRequired();

        constructor.Ignore(producto => producto.Eventos);
    }
}
