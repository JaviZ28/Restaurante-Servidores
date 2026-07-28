using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentas;

public sealed record ObtenerVentasConsulta
    : IConsulta<ResultadoAplicacion<IReadOnlyCollection<VentaDto>>>;
