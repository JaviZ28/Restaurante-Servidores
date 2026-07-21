using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.CancelarVenta;

public sealed class CancelarVentaHandler : IComandoHandler<CancelarVentaComando, ResultadoAplicacion<VentaDto>>
{
    private readonly IRepositorioVenta _repositorioVenta;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CancelarVentaHandler(
        IRepositorioVenta repositorioVenta,
        IUnidadDeTrabajo unidadDeTrabajo,
        IReloj reloj)
    {
        _repositorioVenta = repositorioVenta;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<ResultadoAplicacion<VentaDto>> ManejarAsync(
        CancelarVentaComando comando,
        CancellationToken cancellationToken = default)
    {
        if (comando.VentaId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresVenta.IdInvalido.Codigo, ErroresVenta.IdInvalido.Mensaje);
        }

        var venta = await _repositorioVenta.ObtenerPorIdAsync(comando.VentaId, cancellationToken);
        if (venta is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo("Venta.NoEncontrada", "La venta indicada no existe.");
        }

        var resultado = venta.Cancelar(_reloj.UtcNow);
        if (!resultado.EsExito)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(resultado.Error!.Codigo, resultado.Error.Mensaje);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return ResultadoAplicacion<VentaDto>.Exito(venta.ADto());
    }
}
