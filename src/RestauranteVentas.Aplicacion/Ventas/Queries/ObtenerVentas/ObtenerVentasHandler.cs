using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentas;

public sealed class ObtenerVentasHandler(IVentaLectura lecturaVenta)
    : IConsultaHandler<ObtenerVentasConsulta, ResultadoAplicacion<IReadOnlyCollection<VentaDto>>>
{
    public async Task<ResultadoAplicacion<IReadOnlyCollection<VentaDto>>> ManejarAsync(
        ObtenerVentasConsulta consulta,
        CancellationToken cancellationToken = default) =>
        ResultadoAplicacion<IReadOnlyCollection<VentaDto>>.Exito(
            await lecturaVenta.ObtenerTodasAsync(cancellationToken));
}
