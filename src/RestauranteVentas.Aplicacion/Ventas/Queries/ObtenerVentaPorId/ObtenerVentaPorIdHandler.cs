using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentaPorId;

public sealed class ObtenerVentaPorIdHandler : IConsultaHandler<ObtenerVentaPorIdConsulta, ResultadoAplicacion<VentaDto>>
{
    private readonly IRepositorioVenta _repositorioVenta;

    public ObtenerVentaPorIdHandler(IRepositorioVenta repositorioVenta) =>
        _repositorioVenta = repositorioVenta;

    public async Task<ResultadoAplicacion<VentaDto>> ManejarAsync(
        ObtenerVentaPorIdConsulta consulta,
        CancellationToken cancellationToken = default)
    {
        if (consulta.VentaId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresVenta.IdInvalido.Codigo, ErroresVenta.IdInvalido.Mensaje);
        }

        var venta = await _repositorioVenta.ObtenerPorIdAsync(consulta.VentaId, cancellationToken);
        if (venta is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo("Venta.NoEncontrada", "La venta indicada no existe.");
        }

        return ResultadoAplicacion<VentaDto>.Exito(venta.ADto());
    }
}
