using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.AgregarProductoVenta;

public sealed record AgregarProductoVentaComando(
    Guid VentaId,
    Guid ProductoMenuId,
    int Cantidad)
    : IComando<ResultadoAplicacion<VentaDto>>;
