using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Commands.CambiarCantidadDetalleVenta;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Commands;

public class CambiarCantidadDetalleVentaHandlerTests
{
    [Fact]
    public async Task ManejarAsync_actualiza_cantidad_y_total_de_venta()
    {
        var venta = FabricaDominioPruebas.CrearVentaConDetalle(precio: 10m, cantidad: 1);
        var detalleId = venta.Detalles.Single().Id;
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();

        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        var handler = new CambiarCantidadDetalleVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(
            new CambiarCantidadDetalleVentaComando(venta.Id, detalleId, 3));

        Assert.True(resultado.EsExito);
        Assert.Equal(30m, resultado.Valor!.Total);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManejarAsync_rechaza_nueva_cantidad_invalida()
    {
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();

        var handler = new CambiarCantidadDetalleVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(
            new CambiarCantidadDetalleVentaComando(Guid.NewGuid(), Guid.NewGuid(), 0));

        Assert.False(resultado.EsExito);
        Assert.Equal("Cantidad.Invalida", resultado.Error!.Codigo);
        repositorioVenta.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
