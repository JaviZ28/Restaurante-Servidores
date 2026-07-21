using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Commands.PagarVenta;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Commands;

public class PagarVentaHandlerTests
{
    [Fact]
    public async Task ManejarAsync_rechaza_metodo_pago_invalido()
    {
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var reloj = new Mock<IReloj>();

        var handler = new PagarVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object,
            reloj.Object);

        var resultado = await handler.ManejarAsync(new PagarVentaComando(Guid.NewGuid(), "Crypto"));

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.MetodoPagoInvalido.Codigo, resultado.Error!.Codigo);
        repositorioVenta.Verify(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ManejarAsync_paga_venta_con_metodo_valido()
    {
        var fechaPago = new DateTime(2026, 3, 15, 11, 0, 0, DateTimeKind.Utc);
        var venta = FabricaDominioPruebas.CrearVentaConDetalle();
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var reloj = new Mock<IReloj>();

        reloj.SetupGet(x => x.UtcNow).Returns(fechaPago);
        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        var handler = new PagarVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object,
            reloj.Object);

        var resultado = await handler.ManejarAsync(new PagarVentaComando(venta.Id, "Tarjeta"));

        Assert.True(resultado.EsExito);
        Assert.Equal("Pagada", resultado.Valor!.Estado);
        Assert.Equal("Tarjeta", resultado.Valor.MetodoPago);
        Assert.Equal(fechaPago, resultado.Valor.FechaPagoUtc);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
