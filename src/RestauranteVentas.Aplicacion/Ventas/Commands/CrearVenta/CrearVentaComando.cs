using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.CrearVenta;

public sealed record CrearVentaComando(
    Guid? ClienteId,
    int? NumeroMesa)
    : IComando<ResultadoAplicacion<VentaDto>>;
