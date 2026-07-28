using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.CambiarCantidadDetalleVenta;

public sealed class CambiarCantidadDetalleVentaHandler : IComandoHandler<CambiarCantidadDetalleVentaComando, ResultadoAplicacion<VentaDto>>
{
    private readonly IRepositorioVenta _repositorioVenta;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public CambiarCantidadDetalleVentaHandler(
        IRepositorioVenta repositorioVenta,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _repositorioVenta = repositorioVenta;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<ResultadoAplicacion<VentaDto>> ManejarAsync(
        CambiarCantidadDetalleVentaComando comando,
        CancellationToken cancellationToken = default)
    {
        if (comando.VentaId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresAplicacion.DesdeDominio(ErroresVenta.IdInvalido));
        }

        var resultadoCantidad = Cantidad.Crear(comando.NuevaCantidad);
        if (!resultadoCantidad.EsExito)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresAplicacion.DesdeDominio(resultadoCantidad.Error!));
        }

        var venta = await _repositorioVenta.ObtenerPorIdAsync(comando.VentaId, cancellationToken);
        if (venta is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(
                ErroresAplicacion.NoEncontrado("Venta.NoEncontrada", "La venta indicada no existe."));
        }

        var resultado = venta.CambiarCantidad(comando.DetalleId, resultadoCantidad.Valor);
        if (!resultado.EsExito)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresAplicacion.DesdeDominio(resultado.Error!));
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return ResultadoAplicacion<VentaDto>.Exito(venta.ADto());
    }
}
