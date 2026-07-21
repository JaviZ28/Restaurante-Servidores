using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Mapeadores;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductoMenuPorId;

public sealed class ObtenerProductoMenuPorIdHandler : IConsultaHandler<ObtenerProductoMenuPorIdConsulta, ResultadoAplicacion<ProductoMenuDto>>
{
    private readonly IRepositorioProductoMenu _repositorioProductoMenu;

    public ObtenerProductoMenuPorIdHandler(IRepositorioProductoMenu repositorioProductoMenu) =>
        _repositorioProductoMenu = repositorioProductoMenu;

    public async Task<ResultadoAplicacion<ProductoMenuDto>> ManejarAsync(
        ObtenerProductoMenuPorIdConsulta consulta,
        CancellationToken cancellationToken = default)
    {
        if (consulta.ProductoMenuId == Guid.Empty)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo(ErroresProductoMenu.IdInvalido.Codigo, ErroresProductoMenu.IdInvalido.Mensaje);
        }

        var producto = await _repositorioProductoMenu.ObtenerPorIdAsync(consulta.ProductoMenuId, cancellationToken);
        if (producto is null)
        {
            return ResultadoAplicacion<ProductoMenuDto>.Fallo("ProductoMenu.NoEncontrado", "El producto indicado no existe.");
        }

        return ResultadoAplicacion<ProductoMenuDto>.Exito(producto.ADto());
    }
}
