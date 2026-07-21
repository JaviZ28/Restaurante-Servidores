namespace RestauranteVentas.Dominio.Ventas;

public static class ErroresVenta
{
    public static readonly Abstracciones.Error IdInvalido =
        new("Venta.IdInvalido", "El identificador de la venta es obligatorio.");

    public static readonly Abstracciones.Error ProductoInvalido =
        new("Venta.ProductoInvalido", "El producto es obligatorio.");

    public static readonly Abstracciones.Error CantidadInvalida =
        new("Venta.CantidadInvalida", "La cantidad es obligatoria.");

    public static readonly Abstracciones.Error MetodoPagoInvalido =
        new("Venta.MetodoPagoInvalido", "El método de pago indicado no es válido.");

    public static readonly Abstracciones.Error VentaNoAbierta =
        new("Venta.NoAbierta", "La venta no está abierta.");

    public static readonly Abstracciones.Error SinDetalles =
        new("Venta.SinDetalles", "La venta debe contener al menos un detalle para poder pagarse.");

    public static readonly Abstracciones.Error ProductoInactivo =
        new("Venta.ProductoInactivo", "Un producto inactivo no puede agregarse a una venta.");

    public static readonly Abstracciones.Error DetalleNoEncontrado =
        new("Venta.DetalleNoEncontrado", "El detalle indicado no existe en la venta.");

    public static readonly Abstracciones.Error YaPagada =
        new("Venta.YaPagada", "Una venta pagada no puede modificarse ni cancelarse.");

    public static readonly Abstracciones.Error YaCancelada =
        new("Venta.YaCancelada", "Una venta cancelada no puede modificarse ni pagarse.");
}
