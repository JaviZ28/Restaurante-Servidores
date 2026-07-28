using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Productos.Commands.ActualizarProductoMenu;

public sealed record ActualizarProductoMenuComando(
    Guid ProductoMenuId,
    string Nombre,
    decimal Precio)
    : IComando<ResultadoAplicacion<ProductoMenuDto>>;
