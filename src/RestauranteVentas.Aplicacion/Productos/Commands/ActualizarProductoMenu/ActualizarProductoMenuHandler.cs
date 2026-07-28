using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Productos.Commands.ActualizarProductoMenu;

/// <summary>
/// Actualiza datos vigentes del catálogo. No altera snapshots ya incluidos en ventas.
/// </summary>
public sealed class ActualizarProductoMenuHandler
    : IComandoHandler<ActualizarProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>>
{
    private readonly IRepositorioProductoMenu _repositorioProductoMenu;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;

    public ActualizarProductoMenuHandler(
        IRepositorioProductoMenu repositorioProductoMenu,
        IUnidadDeTrabajo unidadDeTrabajo)
    {
        _repositorioProductoMenu = repositorioProductoMenu;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<ResultadoAplicacion<ProductoMenuDto>> ManejarAsync(
        ActualizarProductoMenuComando comando,
        CancellationToken cancellationToken = default)
    {
        if (comando.ProductoMenuId == Guid.Empty)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(
                ErroresAplicacion.DesdeDominio(ErroresProductoMenu.IdInvalido));
        }

        var nombre = NombreProducto.Crear(comando.Nombre);
        if (!nombre.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(ErroresAplicacion.DesdeDominio(nombre.Error!));
        }

        var precio = Dinero.Crear(comando.Precio);
        if (!precio.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(ErroresAplicacion.DesdeDominio(precio.Error!));
        }

        var producto = await _repositorioProductoMenu.ObtenerPorIdAsync(comando.ProductoMenuId, cancellationToken);
        if (producto is null)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(
                ErroresAplicacion.NoEncontrado("ProductoMenu.NoEncontrado", "El producto indicado no existe."));
        }

        var resultadoNombre = producto.CambiarNombre(nombre.Valor);
        if (!resultadoNombre.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(ErroresAplicacion.DesdeDominio(resultadoNombre.Error!));
        }

        var resultadoPrecio = producto.ActualizarPrecio(precio.Valor);
        if (!resultadoPrecio.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(ErroresAplicacion.DesdeDominio(resultadoPrecio.Error!));
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return ResultadoAplicacion<ProductoMenuDto>.Exito(producto.ADto());
    }
}
