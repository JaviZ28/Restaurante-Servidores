using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Ventas.Commands.CrearVenta;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Commands;

public class CrearVentaHandlerTests
{
    [Fact]
    public async Task ManejarAsync_crea_venta_y_persiste()
    {
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var reloj = new Mock<IReloj>();
        var generadorIdentidad = new Mock<IGeneradorIdentidad>();
        var ventaId = Guid.NewGuid();
        var fecha = new DateTime(2026, 3, 20, 9, 0, 0, DateTimeKind.Utc);

        reloj.SetupGet(x => x.UtcNow).Returns(fecha);
        generadorIdentidad.Setup(x => x.Nuevo()).Returns(ventaId);
        repositorioVenta
            .Setup(x => x.AgregarAsync(It.IsAny<Venta>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CrearVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object,
            reloj.Object,
            generadorIdentidad.Object);

        var resultado = await handler.ManejarAsync(new CrearVentaComando(null, 4));

        Assert.True(resultado.EsExito);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(ventaId, resultado.Valor!.Id);
        Assert.Equal(4, resultado.Valor.NumeroMesa);
        Assert.Equal("Abierta", resultado.Valor.Estado);
        repositorioVenta.Verify(x => x.AgregarAsync(It.IsAny<Venta>(), It.IsAny<CancellationToken>()), Times.Once);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManejarAsync_rechaza_numero_mesa_invalido()
    {
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();
        var reloj = new Mock<IReloj>();
        var generadorIdentidad = new Mock<IGeneradorIdentidad>();

        var handler = new CrearVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object,
            reloj.Object,
            generadorIdentidad.Object);

        var resultado = await handler.ManejarAsync(new CrearVentaComando(null, 0));

        Assert.False(resultado.EsExito);
        Assert.Equal("NumeroMesa.Invalido", resultado.Error!.Codigo);
        repositorioVenta.Verify(x => x.AgregarAsync(It.IsAny<Venta>(), It.IsAny<CancellationToken>()), Times.Never);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
