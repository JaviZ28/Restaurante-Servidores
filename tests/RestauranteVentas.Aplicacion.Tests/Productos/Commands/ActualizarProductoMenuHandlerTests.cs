using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Productos.Commands.ActualizarProductoMenu;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Tests.Productos.Commands;

public sealed class ActualizarProductoMenuHandlerTests
{
    [Fact]
    public async Task ManejarAsync_actualiza_datos_vigentes_sin_recrear_el_producto()
    {
        var producto = FabricaDominioPruebas.CrearProducto("Café", 2m);
        var repositorio = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        repositorio.Setup(x => x.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(producto);

        var handler = new ActualizarProductoMenuHandler(repositorio.Object, unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(
            new ActualizarProductoMenuComando(producto.Id, "Café americano", 2.50m));

        Assert.True(resultado.EsExito);
        Assert.Equal("Café americano", resultado.Valor!.Nombre);
        Assert.Equal(2.50m, resultado.Valor.PrecioActual);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManejarAsync_retorna_no_encontrado_si_el_producto_no_existe()
    {
        var repositorio = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        repositorio.Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductoMenu?)null);

        var handler = new ActualizarProductoMenuHandler(repositorio.Object, unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(
            new ActualizarProductoMenuComando(Guid.NewGuid(), "Café", 2m));

        Assert.False(resultado.EsExito);
        Assert.Equal(TipoErrorAplicacion.NoEncontrado, resultado.Error!.Tipo);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
