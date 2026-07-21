using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Commands.AgregarProductoVenta;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Commands;

public class AgregarProductoVentaHandlerTests
{
    [Fact]
    public async Task ManejarAsync_agrega_producto_a_venta_abierta()
    {
        var venta = FabricaDominioPruebas.CrearVentaAbierta();
        var producto = FabricaDominioPruebas.CrearProducto("Hamburguesa", 18m);

        var repositorioVenta = new Mock<IRepositorioVenta>();
        var repositorioProducto = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();

        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        repositorioProducto
            .Setup(x => x.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        var handler = new AgregarProductoVentaHandler(
            repositorioVenta.Object,
            repositorioProducto.Object,
            unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(new AgregarProductoVentaComando(venta.Id, producto.Id, 2));

        Assert.True(resultado.EsExito);
        Assert.NotNull(resultado.Valor);
        Assert.Single(resultado.Valor!.Detalles);
        Assert.Equal(36m, resultado.Valor.Total);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManejarAsync_retorna_fallo_si_venta_no_existe()
    {
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var repositorioProducto = new Mock<IRepositorioProductoMenu>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();

        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venta?)null);

        var handler = new AgregarProductoVentaHandler(
            repositorioVenta.Object,
            repositorioProducto.Object,
            unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(
            new AgregarProductoVentaComando(Guid.NewGuid(), Guid.NewGuid(), 1));

        Assert.False(resultado.EsExito);
        Assert.Equal("Venta.NoEncontrada", resultado.Error!.Codigo);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
