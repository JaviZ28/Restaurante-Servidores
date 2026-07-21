using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Commands.EliminarDetalleVenta;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Commands;

public class EliminarDetalleVentaHandlerTests
{
    [Fact]
    public async Task ManejarAsync_elimina_detalle_de_venta()
    {
        var venta = FabricaDominioPruebas.CrearVentaConDetalle(cantidad: 2);
        var detalleId = venta.Detalles.Single().Id;
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();

        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        var handler = new EliminarDetalleVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(new EliminarDetalleVentaComando(venta.Id, detalleId));

        Assert.True(resultado.EsExito);
        Assert.Empty(resultado.Valor!.Detalles);
        Assert.Null(resultado.Valor.Total);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManejarAsync_retorna_fallo_si_detalle_no_existe()
    {
        var venta = FabricaDominioPruebas.CrearVentaConDetalle();
        var repositorioVenta = new Mock<IRepositorioVenta>();
        var unidadDeTrabajo = new Mock<IUnidadDeTrabajo>();

        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        var handler = new EliminarDetalleVentaHandler(
            repositorioVenta.Object,
            unidadDeTrabajo.Object);

        var resultado = await handler.ManejarAsync(
            new EliminarDetalleVentaComando(venta.Id, Guid.NewGuid()));

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.DetalleNoEncontrado.Codigo, resultado.Error!.Codigo);
        unidadDeTrabajo.Verify(x => x.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
