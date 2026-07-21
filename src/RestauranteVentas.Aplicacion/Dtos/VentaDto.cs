namespace RestauranteVentas.Aplicacion.Dtos;

public sealed record VentaDto(
    Guid Id,
    Guid? ClienteId,
    int? NumeroMesa,
    string Estado,
    DateTime FechaCreacionUtc,
    DateTime? FechaPagoUtc,
    string? MetodoPago,
    decimal? Total,
    string? MonedaTotal,
    IReadOnlyCollection<DetalleVentaDto> Detalles);
