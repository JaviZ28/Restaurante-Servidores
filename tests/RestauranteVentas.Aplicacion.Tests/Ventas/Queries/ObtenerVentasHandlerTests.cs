using Moq;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Tests.Helpers;
using RestauranteVentas.Aplicacion.Ventas.Queries;
using RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentas;

namespace RestauranteVentas.Aplicacion.Tests.Ventas.Queries;

public class ObtenerVentasHandlerTests
{
    [Fact]
    public async Task ManejarAsync_retorna_el_historial_de_ventas_de_la_lectura()
    {
        IReadOnlyCollection<VentaDto> ventas =
        [
            new VentaDto(
                Guid.NewGuid(),
                null,
                6,
                "Pagada",
                FabricaDominioPruebas.FechaFijaUtc,
                FabricaDominioPruebas.FechaFijaUtc.AddMinutes(3),
                null,
                "Tarjeta",
                null,
                18m,
                "USD",
                [])
        ];
        var lectura = new Mock<IVentaLectura>();
        lectura
            .Setup(x => x.ObtenerTodasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ventas);
        var handler = new ObtenerVentasHandler(lectura.Object);

        var resultado = await handler.ManejarAsync(new ObtenerVentasConsulta());

        Assert.True(resultado.EsExito);
        Assert.Equal(ventas, resultado.Valor);
    }
}
