using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Productos.Commands.CambiarEstadoProductoMenu;

/// <summary>
/// Cambia la disponibilidad futura de un producto sin afectar los precios
/// históricos ya guardados en los detalles de ventas.
/// </summary>
public sealed class CambiarEstadoProductoMenuHandler
    : IComandoHandler<CambiarEstadoProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>>
{
    private readonly IRepositorioProductoMenu _repositorioProductoMenu;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public CambiarEstadoProductoMenuHandler(
        IRepositorioProductoMenu repositorioProductoMenu,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _repositorioProductoMenu = repositorioProductoMenu;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<ResultadoAplicacion<ProductoMenuDto>> ManejarAsync(
        CambiarEstadoProductoMenuComando comando,
        CancellationToken cancellationToken = default)
    {
        if (comando.ProductoMenuId == Guid.Empty)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(
                ErroresAplicacion.DesdeDominio(ErroresProductoMenu.IdInvalido));
        }

        var producto = await _repositorioProductoMenu.ObtenerPorIdAsync(comando.ProductoMenuId, cancellationToken);
        if (producto is null)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(
                ErroresAplicacion.NoEncontrado("ProductoMenu.NoEncontrado", "El producto indicado no existe."));
        }

        var resultado = comando.EstaActivo ? producto.Activar() : producto.Desactivar();
        if (!resultado.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(ErroresAplicacion.DesdeDominio(resultado.Error!));
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return ResultadoAplicacion<ProductoMenuDto>.Exito(producto.ADto());
    }
}
