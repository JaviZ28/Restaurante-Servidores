using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Productos.Commands.CrearProductoMenu;

public sealed class CrearProductoMenuHandler : IComandoHandler<CrearProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>>
{
    private readonly IRepositorioProductoMenu _repositorioProductoMenu;
    private readonly IUnidadDeTrabajo _unidadDeTrabajo;
    private readonly IGeneradorIdentidad _generadorIdentidad;

    public CrearProductoMenuHandler(
        IRepositorioProductoMenu repositorioProductoMenu,
        IUnidadDeTrabajo unidadDeTrabajo,
        IGeneradorIdentidad generadorIdentidad)
    {
        _repositorioProductoMenu = repositorioProductoMenu;
        _unidadDeTrabajo = unidadDeTrabajo;
        _generadorIdentidad = generadorIdentidad;
    }

    public async Task<ResultadoAplicacion<ProductoMenuDto>> ManejarAsync(
        CrearProductoMenuComando comando,
        CancellationToken cancellationToken = default)
    {
        var resultadoNombre = NombreProducto.Crear(comando.Nombre);
        if (!resultadoNombre.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(resultadoNombre.Error!.Codigo, resultadoNombre.Error.Mensaje);
        }

        var resultadoPrecio = Dinero.Crear(comando.Precio);
        if (!resultadoPrecio.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(resultadoPrecio.Error!.Codigo, resultadoPrecio.Error.Mensaje);
        }

        var resultadoProducto = ProductoMenu.Crear(
            _generadorIdentidad.Nuevo(),
            resultadoNombre.Valor,
            resultadoPrecio.Valor);

        if (!resultadoProducto.EsExito)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(resultadoProducto.Error!.Codigo, resultadoProducto.Error.Mensaje);
        }

        var producto = resultadoProducto.Valor!;
        await _repositorioProductoMenu.AgregarAsync(producto, cancellationToken);
        await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return ResultadoAplicacion<ProductoMenuDto>.Exito(producto.ADto());
    }
}
