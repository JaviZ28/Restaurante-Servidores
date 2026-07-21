using System.Net;
using System.Net.Http.Json;

namespace RestauranteVentas.IntegrationTests;

public sealed class FlujoVentaIntegrationTests
{
    private static readonly TimeSpan TiempoEspera = TimeSpan.FromSeconds(45);

    [Fact]
    public async Task La_api_esta_saludable_cuando_el_apphost_inicia()
    {
        await using var aplicacion = await CrearAplicacionAsync();

        using var cliente = aplicacion.CreateHttpClient("api");
        using var respuesta = await cliente.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_producto_venta_y_pago_persiste_el_estado_final()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        var producto = await CrearAsync(
            cliente,
            "/api/productos",
            new { nombre = "Producto de integración", precio = 12.50m });

        var venta = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 4 });

        using var respuestaAgregar = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/detalles",
            new { productoMenuId = producto.Id, cantidad = 2 });

        Assert.Equal(HttpStatusCode.OK, respuestaAgregar.StatusCode);

        using var respuestaPago = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/pagar",
            new { metodoPago = "Tarjeta" });

        Assert.Equal(HttpStatusCode.OK, respuestaPago.StatusCode);

        using var respuestaConsulta = await cliente.GetAsync($"/api/ventas/{venta.Id}");
        var ventaPagada = await respuestaConsulta.Content.ReadFromJsonAsync<VentaRespuesta>();

        Assert.Equal(HttpStatusCode.OK, respuestaConsulta.StatusCode);
        Assert.NotNull(ventaPagada);
        Assert.Equal("Pagada", ventaPagada.Estado);
        Assert.Equal(25.00m, ventaPagada.Total);
    }

    private static async Task<AplicacionIniciada> CrearAplicacionAsync()
    {
        using var cancelacion = new CancellationTokenSource(TiempoEspera);
        var cancellationToken = cancelacion.Token;

        var constructor = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.RestauranteVentas_AppHost>(["UseVolumes=false"], cancellationToken);

        var aplicacion = await constructor.BuildAsync(cancellationToken)
            .WaitAsync(TiempoEspera, cancellationToken);

        await aplicacion.StartAsync(cancellationToken)
            .WaitAsync(TiempoEspera, cancellationToken);

        await aplicacion.ResourceNotifications
            .WaitForResourceHealthyAsync("api", cancellationToken)
            .WaitAsync(TiempoEspera, cancellationToken);

        return new AplicacionIniciada(aplicacion);
    }

    private static async Task<RespuestaCreada> CrearAsync(
        HttpClient cliente,
        string url,
        object solicitud)
    {
        using var respuesta = await cliente.PostAsJsonAsync(url, solicitud);
        var creada = await respuesta.Content.ReadFromJsonAsync<RespuestaCreada>();

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.NotNull(creada);

        return creada;
    }

    private sealed record RespuestaCreada(Guid Id);

    private sealed record VentaRespuesta(Guid Id, string Estado, decimal? Total);

    private sealed class AplicacionIniciada(DistributedApplication aplicacion) : IAsyncDisposable
    {
        public HttpClient CreateHttpClient(string nombreRecurso) => aplicacion.CreateHttpClient(nombreRecurso);

        public ValueTask DisposeAsync() => aplicacion.DisposeAsync();
    }
}
