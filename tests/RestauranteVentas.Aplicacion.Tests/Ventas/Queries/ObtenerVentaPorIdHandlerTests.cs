using Moq;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Queries;
using RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentaPorId;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Queries;

public class ObtenerVentaPorIdHandlerTests
{
    [Fact]
    public async Task ManejarAsync_retorna_venta_cuando_existe()
    {
        var ventaId = Guid.NewGuid();
        var lecturaVenta = new Mock<IVentaLectura>();
        var venta = new VentaDto(
            ventaId,
            null,
            4,
            "Abierta",
            FabricaDominioPruebas.FechaFijaUtc,
            null,
            null,
            null,
            null,
            24m,
            "USD",
            [new DetalleVentaDto(Guid.NewGuid(), Guid.NewGuid(), "Producto", 12m, "USD", 2, 24m)]);

        lecturaVenta
            .Setup(x => x.ObtenerPorIdAsync(ventaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        var handler = new ObtenerVentaPorIdHandler(lecturaVenta.Object);

        var resultado = await handler.ManejarAsync(new ObtenerVentaPorIdConsulta(ventaId));

        Assert.True(resultado.EsExito);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(ventaId, resultado.Valor!.Id);
        Assert.Equal(24m, resultado.Valor.Total);
        Assert.Single(resultado.Valor.Detalles);
    }

    [Fact]
    public async Task ManejarAsync_retorna_fallo_si_venta_no_existe()
    {
        var lecturaVenta = new Mock<IVentaLectura>();
        lecturaVenta
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VentaDto?)null);

        var handler = new ObtenerVentaPorIdHandler(lecturaVenta.Object);

        var resultado = await handler.ManejarAsync(new ObtenerVentaPorIdConsulta(Guid.NewGuid()));

        Assert.False(resultado.EsExito);
        Assert.Equal("Venta.NoEncontrada", resultado.Error!.Codigo);
        Assert.Equal(TipoErrorAplicacion.NoEncontrado, resultado.Error.Tipo);
    }
}
