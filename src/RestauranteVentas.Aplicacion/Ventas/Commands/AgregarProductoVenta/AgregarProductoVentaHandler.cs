using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Ventas.Commands.AgregarProductoVenta;

public sealed class AgregarProductoVentaHandler : IComandoHandler<AgregarProductoVentaComando, ResultadoAplicacion<VentaDto>>
{
    private readonly IRepositorioVenta _repositorioVenta;
    private readonly IRepositorioProductoMenu _repositorioProductoMenu;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public AgregarProductoVentaHandler(
        IRepositorioVenta repositorioVenta,
        IRepositorioProductoMenu repositorioProductoMenu,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _repositorioVenta = repositorioVenta;
        _repositorioProductoMenu = repositorioProductoMenu;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<ResultadoAplicacion<VentaDto>> ManejarAsync(
        AgregarProductoVentaComando comando,
        CancellationToken cancellationToken = default)
    {
        if (comando.VentaId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresAplicacion.DesdeDominio(ErroresVenta.IdInvalido));
        }

        if (comando.ProductoMenuId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresAplicacion.DesdeDominio(ErroresProductoMenu.IdInvalido));
        }

        var resultadoCantidad = Cantidad.Crear(comando.Cantidad);
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

        var producto = await _repositorioProductoMenu.ObtenerPorIdAsync(comando.ProductoMenuId, cancellationToken);
        if (producto is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(
                ErroresAplicacion.NoEncontrado("ProductoMenu.NoEncontrado", "El producto indicado no existe."));
        }

        var resultado = venta.AgregarProducto(producto, resultadoCantidad.Valor);
        if (!resultado.EsExito)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresAplicacion.DesdeDominio(resultado.Error!));
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return ResultadoAplicacion<VentaDto>.Exito(venta.ADto());
    }
}
