using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Aplicacion.Productos.Queries;

namespace RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductoMenuPorId;

public sealed class ObtenerProductoMenuPorIdHandler : IConsultaHandler<ObtenerProductoMenuPorIdConsulta, ResultadoAplicacion<ProductoMenuDto>>
{
    private readonly IProductoMenuLectura _lecturaProductoMenu;

    public ObtenerProductoMenuPorIdHandler(IProductoMenuLectura lecturaProductoMenu) =>
        _lecturaProductoMenu = lecturaProductoMenu;

    public async Task<ResultadoAplicacion<ProductoMenuDto>> ManejarAsync(
        ObtenerProductoMenuPorIdConsulta consulta,
        CancellationToken cancellationToken = default)
    {
        if (consulta.ProductoMenuId == Guid.Empty)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(ErroresAplicacion.DesdeDominio(ErroresProductoMenu.IdInvalido));
        }

        var producto = await _lecturaProductoMenu.ObtenerPorIdAsync(consulta.ProductoMenuId, cancellationToken);
        if (producto is null)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(
                ErroresAplicacion.NoEncontrado("ProductoMenu.NoEncontrado", "El producto indicado no existe."));
        }

        return ResultadoAplicacion<ProductoMenuDto>.Exito(producto);
    }
}
