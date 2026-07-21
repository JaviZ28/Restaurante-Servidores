using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Helpers;

internal static class FabricaDominioPruebas
{
    public static readonly DateTime FechaFijaUtc = new(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);

    public static ProductoMenu CrearProducto(string nombre = "Producto", decimal precio = 10m)
    {
        var resultado = ProductoMenu.Crear(
            Guid.NewGuid(),
            NombreProducto.Crear(nombre).Valor!,
            Dinero.Crear(precio).Valor!);

        return resultado.Valor!;
    }

    public static Venta CrearVentaAbierta(Guid? ventaId = null)
    {
        var resultado = Venta.Crear(ventaId ?? Guid.NewGuid(), null, null, FechaFijaUtc);
        return resultado.Valor!;
    }

    public static Venta CrearVentaConDetalle(
        string nombre = "Producto",
        decimal precio = 10m,
        int cantidad = 1,
        Guid? ventaId = null)
    {
        var venta = CrearVentaAbierta(ventaId);
        var producto = CrearProducto(nombre, precio);
        var cantidadVo = Cantidad.Crear(cantidad).Valor!;
        venta.AgregarProducto(producto, cantidadVo);
        return venta;
    }
}
