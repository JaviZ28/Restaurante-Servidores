using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Productos.Commands.CambiarEstadoProductoMenu;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Tests.Productos.Commands;

public sealed class CambiarEstadoProductoMenuHandlerTests
{
    [Fact]
    public async Task ManejarAsync_desactiva_producto_y_persiste_el_cambio()
    {
        var producto = FabricaDominioPruebas.CrearProducto();
        var repositorio = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        repositorio.Setup(x => x.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(producto);

        var handler = new CambiarEstadoProductoMenuHandler(repositorio.Object, unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(new CambiarEstadoProductoMenuComando(producto.Id, false));

        Assert.True(resultado.EsExito);
        Assert.False(resultado.Valor!.EstaActivo);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManejarAsync_retorna_no_encontrado_cuando_no_hay_producto()
    {
        var repositorio = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        repositorio.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductoMenu?)null);

        var handler = new CambiarEstadoProductoMenuHandler(repositorio.Object, unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(new CambiarEstadoProductoMenuComando(Guid.NewGuid(), false));

        Assert.False(resultado.EsExito);
        Assert.Equal(TipoErrorAplicacion.NoEncontrado, resultado.Error!.Tipo);
    }
}
