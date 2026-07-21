using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Productos.Commands.CrearProductoMenu;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Tests.Productos.Commands;

public class CrearProductoMenuHandlerTests
{
    [Fact]
    public async Task ManejarAsync_crea_producto_y_persiste()
    {
        var repositorio = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var generadorIdentidad = new Mock<IGeneradorIdentidad>();
        var productoId = Guid.NewGuid();

        generadorIdentidad.Setup(x => x.Nuevo()).Returns(productoId);
        repositorio
            .Setup(x => x.AgregarAsync(It.IsAny<ProductoMenu>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CrearProductoMenuHandler(
            repositorio.Object,
            unidadDeTrabajo.Object,
            generadorIdentidad.Object);

        var resultado = await handler.ManejarAsync(new CrearProductoMenuComando("Pizza Margarita", 32m));

        Assert.True(resultado.EsExito);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(productoId, resultado.Valor!.Id);
        Assert.Equal("Pizza Margarita", resultado.Valor.Nombre);
        Assert.Equal(32m, resultado.Valor.PrecioActual);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManejarAsync_rechaza_nombre_invalido()
    {
        var repositorio = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var generadorIdentidad = new Mock<IGeneradorIdentidad>();

        var handler = new CrearProductoMenuHandler(
            repositorio.Object,
            unidadDeTrabajo.Object,
            generadorIdentidad.Object);

        var resultado = await handler.ManejarAsync(new CrearProductoMenuComando(" ", 20m));

        Assert.False(resultado.EsExito);
        Assert.Equal("NombreProducto.Vacio", resultado.Error!.Codigo);
        repositorio.Verify(x => x.AgregarAsync(It.IsAny<ProductoMenu>(), It.IsAny<CancellationToken>()), Times.Never);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
