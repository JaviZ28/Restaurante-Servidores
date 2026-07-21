using Moq;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentaPorId;
using RestauranteVentas.Dominio.Ventas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Queries;

public class ObtenerVentaPorIdHandlerTests
{
    [Fact]
    public async Task ManejarAsync_retorna_venta_cuando_existe()
    {
        var venta = FabricaDominioPruebas.CrearVentaConDetalle(precio: 12m, cantidad: 2);
        var repositorioVenta = new Mock<IRepositorioVenta>();

        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);

        var handler = new ObtenerVentaPorIdHandler(repositorioVenta.Object);

        var resultado = await handler.ManejarAsync(new ObtenerVentaPorIdConsulta(venta.Id));

        Assert.True(resultado.EsExito);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(venta.Id, resultado.Valor!.Id);
        Assert.Equal(24m, resultado.Valor.Total);
        Assert.Single(resultado.Valor.Detalles);
    }

    [Fact]
    public async Task ManejarAsync_retorna_fallo_si_venta_no_existe()
    {
        var repositorioVenta = new Mock<IRepositorioVenta>();
        repositorioVenta
            .Setup(x => x.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Venta?)null);

        var handler = new ObtenerVentaPorIdHandler(repositorioVenta.Object);

        var resultado = await handler.ManejarAsync(new ObtenerVentaPorIdConsulta(Guid.NewGuid()));

        Assert.False(resultado.EsExito);
        Assert.Equal("Venta.NoEncontrada", resultado.Error!.Codigo);
    }
}
