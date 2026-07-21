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
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresVenta.IdInvalido.Codigo, ErroresVenta.IdInvalido.Mensaje);
        }

        if (comando.ProductoMenuId == Guid.Empty)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(ErroresProductoMenu.IdInvalido.Codigo, ErroresProductoMenu.IdInvalido.Mensaje);
        }

        var resultadoCantidad = Cantidad.Crear(comando.Cantidad);
        if (!resultadoCantidad.EsExito)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(resultadoCantidad.Error!.Codigo, resultadoCantidad.Error.Mensaje);
        }

        var venta = await _repositorioVenta.ObtenerPorIdAsync(comando.VentaId, cancellationToken);
        if (venta is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo("Venta.NoEncontrada", "La venta indicada no existe.");
        }

        var producto = await _repositorioProductoMenu.ObtenerPorIdAsync(comando.ProductoMenuId, cancellationToken);
        if (producto is null)
        {
            return ResultadoAplicacion<VentaDto>.Fallo("ProductoMenu.NoEncontrado", "El producto indicado no existe.");
        }

        var resultado = venta.AgregarProducto(producto, resultadoCantidad.Valor);
        if (!resultado.EsExito)
        {
            return ResultadoAplicacion<VentaDto>.Fallo(resultado.Error!.Codigo, resultado.Error.Mensaje);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return ResultadoAplicacion<VentaDto>.Exito(venta.ADto());
    }
}
