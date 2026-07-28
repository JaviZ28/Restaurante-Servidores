namespace RestauranteVentas.Dominio.Ventas;

public static class ErroresVenta
{
    public static readonly Abstracciones.Error IdInvalido =
        new("Venta.IdInvalido", "El identificador de la venta es obligatorio.");

    public static readonly Abstracciones.Error ClienteIdInvalido =
        new("Venta.ClienteIdInvalido", "El identificador del cliente no puede estar vacío.");

    public static readonly Abstracciones.Error FechaCreacionInvalida =
        new("Venta.FechaCreacionInvalida", "La fecha de creación debe ser un instante UTC válido.");

    public static readonly Abstracciones.Error ProductoInvalido =
        new("Venta.ProductoInvalido", "El producto es obligatorio.");

    public static readonly Abstracciones.Error CantidadInvalida =
        new("Venta.CantidadInvalida", "La cantidad es obligatoria.");

    public static readonly Abstracciones.Error MetodoPagoInvalido =
        new("Venta.MetodoPagoInvalido", "El método de pago indicado no es válido.");

    public static readonly Abstracciones.Error FechaPagoInvalida =
        new("Venta.FechaPagoInvalida", "La fecha de pago debe ser un instante UTC válido.");

    public static readonly Abstracciones.Error FechaPagoNoPosteriorACreacion =
        new("Venta.FechaPagoNoPosteriorACreacion", "La fecha de pago debe ser posterior a la creación de la venta.");

    public static readonly Abstracciones.Error FechaCancelacionInvalida =
        new("Venta.FechaCancelacionInvalida", "La fecha de cancelación debe ser un instante UTC válido.");

    public static readonly Abstracciones.Error FechaCancelacionNoPosteriorACreacion =
        new("Venta.FechaCancelacionNoPosteriorACreacion", "La fecha de cancelación debe ser posterior a la creación de la venta.");

    public static readonly Abstracciones.Error MotivoCancelacionInvalido =
        new("Venta.MotivoCancelacionInvalido", "El motivo de cancelación es obligatorio y no puede superar los 500 caracteres.");

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
