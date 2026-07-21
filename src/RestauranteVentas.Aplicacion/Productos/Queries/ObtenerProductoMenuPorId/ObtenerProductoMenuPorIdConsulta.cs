using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductoMenuPorId;

public sealed record ObtenerProductoMenuPorIdConsulta(Guid ProductoMenuId)
    : IConsulta<ResultadoAplicacion<ProductoMenuDto>>;
