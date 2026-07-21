using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.CrearVenta;

public sealed class CrearVentaHandler : IComandoHandler<CrearVentaComando, ResultadoAplicacion<VentaDto>>
{
    private readonly IRepositorioVenta _repositorioVenta;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IReloj _reloj;
    private readonly IGeneradorIdentidad _generadorIdentidad;

    public CrearVentaHandler(
        IRepositorioVenta repositorioVenta,
        IUnidadDeTrabajo unidadDeTrabajo,
        IReloj reloj,
        IGeneradorIdentidad generadorIdentidad)
    {
        _repositorioVenta = repositorioVenta;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
        _generadorIdentidad = generadorIdentidad;
    }

    public async Task<ResultadoAplicacion<VentaDto>> ManejarAsync(
        CrearVentaComando comando,
        CancellationToken cancellationToken = default)
    {
        NumeroMesa? mesa = null;

        if (comando.NumeroMesa.HasValue)
        {
            var resultadoMesa = NumeroMesa.Crear(comando.NumeroMesa.Value);
            if (!resultadoMesa.EsExito)
            {
                return FalloDominio(resultadoMesa.Error!.Codigo, resultadoMesa.Error.Mensaje);
            }

            mesa = resultadoMesa.Valor!;
        }

        var resultadoVenta = Venta.Crear(
            _generadorIdentidad.Nuevo(),
            comando.ClienteId,
            mesa,
            _reloj.UtcNow);

        if (!resultadoVenta.EsExito)
        {
            return FalloDominio(resultadoVenta.Error!.Codigo, resultadoVenta.Error.Mensaje);
        }

        var venta = resultadoVenta.Valor!;
        await _repositorioVenta.AgregarAsync(venta, cancellationToken);
        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return ResultadoAplicacion<VentaDto>.Exito(venta.ADto());
    }

    private static ResultadoAplicacion<VentaDto> FalloDominio(string codigo, string mensaje) =>
        ResultadoAplicacion<VentaDto>.Fallo(codigo, mensaje);
}
