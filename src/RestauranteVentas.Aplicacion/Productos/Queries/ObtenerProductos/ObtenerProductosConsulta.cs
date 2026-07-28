using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductos;

public sealed record ObtenerProductosConsulta
    : IConsulta<ResultadoAplicacion<IReadOnlyCollection<ProductoMenuDto>>>;
