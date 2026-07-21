using RestauranteVentas.Dominio.Abstracciones;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Dominio.Ventas;

public sealed class DetalleVenta
{
    public Guid Id { get; }
    public Guid ProductoMenuId { get; }
    public NombreProducto NombreHistorico { get; }
    public Dinero PrecioUnitarioHistorico { get; }
    public Cantidad Cantidad { get; private set; }

    public Dinero Subtotal
    {
        get
        {
            var resultado = PrecioUnitarioHistorico.Multiplicar(Cantidad);
            return resultado.Valor!;
        }
    }

    private DetalleVenta(
        Guid id,
        Guid productoMenuId,
        NombreProducto nombreHistorico,
        Dinero precioUnitarioHistorico,
        Cantidad cantidad)
    {
        Id = id;
        ProductoMenuId = productoMenuId;
        NombreHistorico = nombreHistorico;
        PrecioUnitarioHistorico = precioUnitarioHistorico;
        Cantidad = cantidad;
    }

    internal static Resultado<DetalleVenta> Crear(
        Guid id,
        ProductoMenu producto,
        Cantidad cantidad)
    {
        var detalle = new DetalleVenta(
            id,
            producto.Id,
            producto.Nombre,
            producto.PrecioActual,
            cantidad);

        return Resultado<DetalleVenta>.Exito(detalle);
    }

    internal Resultado CambiarCantidad(Cantidad nuevaCantidad)
    {
        Cantidad = nuevaCantidad;
        return Resultado.Exito();
    }
}
