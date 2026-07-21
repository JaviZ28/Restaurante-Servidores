namespace RestauranteVentas.Aplicacion.Dtos;

public sealed record DetalleVentaDto(
    Guid Id,
    Guid ProductoMenuId,
    string NombreHistorico,
    decimal PrecioUnitario,
    string Moneda,
    int Cantidad,
    decimal Subtotal);
