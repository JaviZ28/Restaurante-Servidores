using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Productos.Queries;
using RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductoMenuPorId;

namespace RestauranteVentas.Aplicacion.Tests.Productos.Queries;

public class ObtenerProductoMenuPorIdHandlerTests
{
    [Fact]
    public async Task ManejarAsync_retorna_producto_cuando_existe()
    {
        var productoId = Guid.NewGuid();
        var producto = new ProductoMenuDto(productoId, "Lasaña", 45m, "USD", true);
        var lectura = new Mock<IProductoMenuLectura>();

        lectura
            .Setup(x => x.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        var handler = new ObtenerProductoMenuPorIdHandler(lectura.Object);

        var resultado = await handler.ManejarAsync(new ObtenerProductoMenuPorIdConsulta(productoId));

        Assert.True(resultado.EsExito);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(productoId, resultado.Valor!.Id);
        Assert.Equal("Lasaña", resultado.Valor.Nombre);
    }

    [Fact]
    public async Task ManejarAsync_retorna_fallo_si_no_existe()
    {
        var lectura = new Mock<IProductoMenuLectura>();
        lectura
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductoMenuDto?)null);

        var handler = new ObtenerProductoMenuPorIdHandler(lectura.Object);

        var resultado = await handler.ManejarAsync(new ObtenerProductoMenuPorIdConsulta(Guid.NewGuid()));

        Assert.False(resultado.EsExito);
        Assert.Equal("ProductoMenu.NoEncontrado", resultado.Error!.Codigo);
        Assert.Equal(TipoErrorAplicacion.NoEncontrado, resultado.Error.Tipo);
    }
}
