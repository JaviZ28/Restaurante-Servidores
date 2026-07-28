using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductos;

public sealed class ObtenerProductosHandler(IProductoMenuLectura lecturaProductoMenu)
    : IConsultaHandler<ObtenerProductosConsulta, ResultadoAplicacion<IReadOnlyCollection<ProductoMenuDto>>>
{
    public async Task<ResultadoAplicacion<IReadOnlyCollection<ProductoMenuDto>>> ManejarAsync(
        ObtenerProductosConsulta consulta,
        CancellationToken cancellationToken = default) =>
        ResultadoAplicacion<IReadOnlyCollection<ProductoMenuDto>>.Exito(
            await lecturaProductoMenu.ObtenerTodosAsync(cancellationToken));
}
