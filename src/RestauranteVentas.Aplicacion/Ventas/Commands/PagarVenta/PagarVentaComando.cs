using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.PagarVenta;

public sealed record PagarVentaComando(
    Guid VentaId,
    string MetodoPago)
    : IComando<ResultadoAplicacion<VentaDto>>;
