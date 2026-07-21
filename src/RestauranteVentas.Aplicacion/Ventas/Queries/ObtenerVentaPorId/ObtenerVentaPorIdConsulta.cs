using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentaPorId;

public sealed record ObtenerVentaPorIdConsulta(Guid VentaId)
    : IConsulta<ResultadoAplicacion<VentaDto>>;
