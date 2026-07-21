using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.CancelarVenta;

public sealed record CancelarVentaComando(Guid VentaId)
    : IComando<ResultadoAplicacion<VentaDto>>;
