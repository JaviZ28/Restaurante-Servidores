using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Productos.Commands.CambiarEstadoProductoMenu;

public sealed record CambiarEstadoProductoMenuComando(Guid ProductoMenuId, bool EstaActivo)
    : IComando<ResultadoAplicacion<ProductoMenuDto>>;
