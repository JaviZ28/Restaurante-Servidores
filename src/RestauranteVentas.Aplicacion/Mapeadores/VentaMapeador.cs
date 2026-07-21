using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Mapeadores;

public static class VentaMapeador
{
    public static VentaDto ADto(this Venta venta)
    {
        var detalles = venta.Detalles.Select(ADto).ToList();
        var total = venta.Total;

        return new VentaDto(
            venta.Id,
            venta.ClienteId,
            venta.Mesa?.Valor,
            venta.Estado.ToString(),
            venta.FechaCreacionUtc,
            venta.FechaPagoUtc,
            venta.MetodoPago?.ToString(),
            total?.Monto,
            total?.Moneda,
            detalles);
    }

    private static DetalleVentaDto ADto(DetalleVenta detalle) =>
        new(
            detalle.Id,
            detalle.ProductoMenuId,
            detalle.NombreHistorico.Valor,
            detalle.PrecioUnitarioHistorico.Monto,
            detalle.PrecioUnitarioHistorico.Moneda,
            detalle.Cantidad.Valor,
            detalle.Subtotal.Monto);
}
