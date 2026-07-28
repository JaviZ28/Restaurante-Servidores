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
        get => CalcularSubtotal(Cantidad).Valor!;
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
        var resultadoSubtotal = producto.PrecioActual.Multiplicar(cantidad);
        if (!resultadoSubtotal.EsExito)
        {
            return Resultado<DetalleVenta>.Fallo(resultadoSubtotal.Error!);
        }

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
        var resultadoSubtotal = CalcularSubtotal(nuevaCantidad);
        if (!resultadoSubtotal.EsExito)
        {
            return Resultado.Fallo(resultadoSubtotal.Error!);
        }

        Cantidad = nuevaCantidad;
        return Resultado.Exito();
    }

    internal Resultado<Dinero> CalcularSubtotal(Cantidad cantidad) =>
        PrecioUnitarioHistorico.Multiplicar(cantidad);
}
