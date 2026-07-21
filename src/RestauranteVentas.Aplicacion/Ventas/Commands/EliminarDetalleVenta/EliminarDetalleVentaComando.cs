using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.EliminarDetalleVenta;

public sealed record EliminarDetalleVentaComando(
    Guid VentaId,
    Guid DetalleId)
    : IComando<ResultadoAplicacion<VentaDto>>;
