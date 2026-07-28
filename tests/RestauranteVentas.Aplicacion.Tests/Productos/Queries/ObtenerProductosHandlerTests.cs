using Moq;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Productos.Queries;
using RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductos;

namespace RestauranteVentas.Aplicacion.Tests.Productos.Queries;

public class ObtenerProductosHandlerTests
{
    [Fact]
    public async Task ManejarAsync_retorna_los_productos_de_la_lectura()
    {
        IReadOnlyCollection<ProductoMenuDto> productos =
        [
            new ProductoMenuDto(Guid.NewGuid(), "Ensalada", 8m, "USD", true),
            new ProductoMenuDto(Guid.NewGuid(), "Sopa", 5m, "USD", false)
        ];
        var lectura = new Mock<IProductoMenuLectura>();
        lectura
            .Setup(x => x.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);
        var handler = new ObtenerProductosHandler(lectura.Object);

        var resultado = await handler.ManejarAsync(new ObtenerProductosConsulta());

        Assert.True(resultado.EsExito);
        Assert.Equal(productos, resultado.Valor);
    }
}
