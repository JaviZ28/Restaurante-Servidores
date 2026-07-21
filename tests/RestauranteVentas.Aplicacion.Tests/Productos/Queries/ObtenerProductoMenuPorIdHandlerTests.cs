using Moq;
using RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductoMenuPorId;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Tests.Productos.Queries;

public class ObtenerProductoMenuPorIdHandlerTests
{
    [Fact]
    public async Task ManejarAsync_retorna_producto_cuando_existe()
    {
        var producto = FabricaDominioPruebas.CrearProducto("Lasaña", 45m);
        var repositorio = new Mock<IRepositorioProductoMenu>();

        repositorio
            .Setup(x => x.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        var handler = new ObtenerProductoMenuPorIdHandler(repositorio.Object);

        var resultado = await handler.ManejarAsync(new ObtenerProductoMenuPorIdConsulta(producto.Id));

        Assert.True(resultado.EsExito);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(producto.Id, resultado.Valor!.Id);
        Assert.Equal("Lasaña", resultado.Valor.Nombre);
    }

    [Fact]
    public async Task ManejarAsync_retorna_fallo_si_no_existe()
    {
        var repositorio = new Mock<IRepositorioProductoMenu>();
        repositorio
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductoMenu?)null);

        var handler = new ObtenerProductoMenuPorIdHandler(repositorio.Object);

        var resultado = await handler.ManejarAsync(new ObtenerProductoMenuPorIdConsulta(Guid.NewGuid()));

        Assert.False(resultado.EsExito);
        Assert.Equal("ProductoMenu.NoEncontrado", resultado.Error!.Codigo);
    }
}
