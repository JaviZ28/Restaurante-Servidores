namespace RestauranteVentas.Aplicacion.Dtos;

public sealed record ProductoMenuDto(
    Guid Id,
    string Nombre,
    decimal PrecioActual,
    string Moneda,
    bool EstaActivo);
