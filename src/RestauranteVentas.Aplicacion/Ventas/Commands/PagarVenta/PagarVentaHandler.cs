using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.PagarVenta;

public sealed class PagarVentaHandler : IComandoHandler<PagarVentaComando, ResultadoAplicacion<VentaDto>>
{
    private readonly IRepositorioVenta _repositorioVenta;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public PagarVentaHandler(
        IRepositorioVenta repositorioVenta,
        IUnidadDeTrabajo unidadDeTrabajo,
        IReloj reloj)
    {
        _repositorioVenta = repositorioVenta;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<ResultadoAplicacion<VentaDto>> ManejarAsync(
        PagarVentaComando comando,
        CancellationToken cancellationToken = default)
    {
        if (comando.VentaId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresVenta.IdInvalido.Codigo, ErroresVenta.IdInvalido.Mensaje);
        }

        if (!Enum.TryParse<MetodoPago>(comando.MetodoPago, true, out var metodoPago))
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresVenta.MetodoPagoInvalido.Codigo, ErroresVenta.MetodoPagoInvalido.Mensaje);
        }

        var venta = await _repositorioVenta.ObtenerPorIdAsync(comando.VentaId, cancellationToken);
        if (venta is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo("Venta.NoEncontrada", "La venta indicada no existe.");
        }

        var resultado = venta.Pagar(metodoPago, _reloj.UtcNow);
        if (!resultado.EsExito)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(resultado.Error!.Codigo, resultado.Error.Mensaje);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return ResultadoAplicacion<VentaDto>.Exito(venta.ADto());
    }
}
