using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Aplicacion.Ventas.Queries;

namespace RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentaPorId;

public sealed class ObtenerVentaPorIdHandler : IConsultaHandler<ObtenerVentaPorIdConsulta, ResultadoAplicacion<VentaDto>>
{
    private readonly IVentaLectura _lecturaVenta;

    public ObtenerVentaPorIdHandler(IVentaLectura lecturaVenta) =>
        _lecturaVenta = lecturaVenta;

    public async Task<ResultadoAplicacion<VentaDto>> ManejarAsync(
        ObtenerVentaPorIdConsulta consulta,
        CancellationToken cancellationToken = default)
    {
        if (consulta.VentaId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresAplicacion.DesdeDominio(ErroresVenta.IdInvalido));
        }

        var venta = await _lecturaVenta.ObtenerPorIdAsync(consulta.VentaId, cancellationToken);
        if (venta is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(
                ErroresAplicacion.NoEncontrado("Venta.NoEncontrada", "La venta indicada no existe."));
        }

        return ResultadoAplicacion<VentaDto>.Exito(venta);
    }
}
