using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Dominio.Ventas.Eventos;

namespace RestauranteVentas.Dominio.Tests.Productos;

public class ProductoMenuTests
{
    private static readonly DateTime FechaFija = new(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Crear_producto_activo_con_datos_validos()
    {
        var id = Guid.NewGuid();
        var nombre = NombreProducto.Crear("Hamburguesa").Valor!;
        var precio = Dinero.Crear(15m).Valor!;

        var resultado = ProductoMenu.Crear(id, nombre, precio);

        Assert.True(resultado.EsExito);
        Assert.Equal(id, resultado.Valor!.Id);
        Assert.True(resultado.Valor.EstaActivo);
        Assert.Equal("Hamburguesa", resultado.Valor.Nombre.Valor);
        Assert.Equal(15m, resultado.Valor.PrecioActual.Monto);
    }

    [Fact]
    public void No_permite_precio_invalido()
    {
        var producto = CrearProductoActivo();

        var resultado = producto.ActualizarPrecio(null);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresProductoMenu.PrecioInvalido.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void Se_puede_desactivar_y_activar()
    {
        var producto = CrearProductoActivo();

        var desactivar = producto.Desactivar();
        Assert.True(desactivar.EsExito);
        Assert.False(producto.EstaActivo);

        var activar = producto.Activar();
        Assert.True(activar.EsExito);
        Assert.True(producto.EstaActivo);
    }

    [Fact]
    public void Producto_desactivado_no_puede_agregarse_a_venta()
    {
        var producto = CrearProductoActivo();
        producto.Desactivar();

        var venta = Venta.Crear(Guid.NewGuid(), null, null, FechaFija).Valor!;
        var cantidad = Cantidad.Crear(1).Valor!;

        var resultado = venta.AgregarProducto(producto, cantidad);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.ProductoInactivo.Codigo, resultado.Error!.Codigo);
    }

    private static ProductoMenu CrearProductoActivo()
    {
        var nombre = NombreProducto.Crear("Pizza").Valor!;
        var precio = Dinero.Crear(20m).Valor!;
        return ProductoMenu.Crear(Guid.NewGuid(), nombre, precio).Valor!;
    }
}
