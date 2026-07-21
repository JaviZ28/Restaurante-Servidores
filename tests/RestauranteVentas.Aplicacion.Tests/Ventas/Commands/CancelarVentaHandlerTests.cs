using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Commands.CancelarVenta;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Commands;

public class CancelarVentaHandlerTests
{
    [Fact]
    public async Task ManejarAsync_retorna_fallo_si_venta_no_existe()
    {
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var reloj = new Mock<IReloj>();

        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venta?)null);

        var handler = new CancelarVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object,
            reloj.Object);

        var resultado = await handler.ManejarAsync(new CancelarVentaComando(Guid.NewGuid()));

        Assert.False(resultado.EsExito);
        Assert.Equal("Venta.NoEncontrada", resultado.Error!.Codigo);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ManejarAsync_cancela_venta_y_persiste_cambio()
    {
        var venta = FabricaDominioPruebas.CrearVentaConDetalle();
        var fechaCancelacion = new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var reloj = new Mock<IReloj>();

        reloj.SetupGet(x => x.UtcNow).Returns(fechaCancelacion);
        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        var handler = new CancelarVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object,
            reloj.Object);

        var resultado = await handler.ManejarAsync(new CancelarVentaComando(venta.Id));

        Assert.True(resultado.EsExito);
        Assert.Equal("Cancelada", resultado.Valor!.Estado);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
