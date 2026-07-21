using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.CambiarCantidadDetalleVenta;

public sealed record CambiarCantidadDetalleVentaComando(
    Guid VentaId,
    Guid DetalleId,
    int NuevaCantidad)
    : IComando<ResultadoAplicacion<VentaDto>>;
