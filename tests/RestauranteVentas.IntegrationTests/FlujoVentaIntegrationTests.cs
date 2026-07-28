using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Infrastructure.Persistencia;

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
    public async Task Listados_de_productos_y_ventas_exponen_el_catalogo_y_el_historial()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        var producto = await CrearAsync(
            cliente,
            "/api/productos",
            new { nombre = "Producto listado", precio = 9.50m });
        var venta = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 3 });

        using var respuestaProductos = await cliente.GetAsync("/api/productos");

        Assert.Equal(HttpStatusCode.OK, respuestaProductos.StatusCode);
        var productos = await respuestaProductos.Content.ReadFromJsonAsync<List<ProductoRespuesta>>();
        Assert.NotNull(productos);
        Assert.Contains(productos, item => item.Id == producto.Id && item.Nombre == "Producto listado");

        using var respuestaVentas = await cliente.GetAsync("/api/ventas");

        Assert.Equal(HttpStatusCode.OK, respuestaVentas.StatusCode);
        var ventas = await respuestaVentas.Content.ReadFromJsonAsync<List<VentaRespuesta>>();
        Assert.NotNull(ventas);
        Assert.Contains(ventas, item => item.Id == venta.Id && item.Estado == "Abierta");
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

    [Fact]
    public async Task Crear_venta_y_cancelarla_persiste_el_estado_final()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        var venta = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 10 });

        using var respuestaCancelar = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/cancelar",
            new { motivoCancelacion = "Cliente desistió de la compra" });

        Assert.Equal(HttpStatusCode.OK, respuestaCancelar.StatusCode);

        using var respuestaConsulta = await cliente.GetAsync($"/api/ventas/{venta.Id}");
        var ventaCancelada = await respuestaConsulta.Content.ReadFromJsonAsync<VentaRespuesta>();

        Assert.Equal(HttpStatusCode.OK, respuestaConsulta.StatusCode);
        Assert.NotNull(ventaCancelada);
        Assert.Equal("Cancelada", ventaCancelada.Estado);
        Assert.Equal("Cliente desistió de la compra", ventaCancelada.MotivoCancelacion);
        Assert.NotNull(ventaCancelada.FechaCancelacionUtc);
    }

    [Fact]
    public async Task Cambio_desactualizado_de_detalle_no_modifica_una_venta_pagada()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        var producto = await CrearAsync(
            cliente,
            "/api/productos",
            new { nombre = "Producto para concurrencia", precio = 8m });
        var ventaCreada = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 11 });

        using var respuestaAgregar = await cliente.PostAsJsonAsync(
            $"/api/ventas/{ventaCreada.Id}/detalles",
            new { productoMenuId = producto.Id, cantidad = 1 });
        Assert.Equal(HttpStatusCode.OK, respuestaAgregar.StatusCode);

        var cadenaConexion = await aplicacion.ObtenerCadenaConexionAsync("restauranteventas");
        Assert.False(string.IsNullOrWhiteSpace(cadenaConexion));

        var opciones = new DbContextOptionsBuilder<RestauranteVentasDbContext>()
            .UseNpgsql(cadenaConexion)
            .Options;

        await using var primerContexto = new RestauranteVentasDbContext(opciones);
        await using var segundoContexto = new RestauranteVentasDbContext(opciones);

        var primeraCopia = await primerContexto.Ventas
            .Include(venta => venta.Detalles)
            .SingleAsync(venta => venta.Id == ventaCreada.Id);
        var segundaCopia = await segundoContexto.Ventas
            .Include(venta => venta.Detalles)
            .SingleAsync(venta => venta.Id == ventaCreada.Id);

        var resultadoPago = primeraCopia.Pagar(
            MetodoPago.Efectivo,
            primeraCopia.FechaCreacionUtc.AddMinutes(1));
        Assert.True(resultadoPago.EsExito);
        await primerContexto.SaveChangesAsync();

        var detalleDesactualizado = Assert.Single(segundaCopia.Detalles);
        var nuevaCantidad = Cantidad.Crear(2).Valor!;
        var resultadoCantidad = segundaCopia.CambiarCantidad(detalleDesactualizado.Id, nuevaCantidad);
        Assert.True(resultadoCantidad.EsExito);

        var unidadDeTrabajo = new UnidadDeTrabajo(segundoContexto);

        var excepcion = await Assert.ThrowsAsync<ConflictoConcurrenciaException>(
            () => unidadDeTrabajo.GuardarCambiosAsync());

        Assert.IsType<DbUpdateConcurrencyException>(excepcion.InnerException);

        await using var contextoVerificador = new RestauranteVentasDbContext(opciones);
        var ventaPersistida = await contextoVerificador.Ventas
            .Include(venta => venta.Detalles)
            .SingleAsync(venta => venta.Id == ventaCreada.Id);

        Assert.Equal(EstadoVenta.Pagada, ventaPersistida.Estado);
        Assert.Equal(1, Assert.Single(ventaPersistida.Detalles).Cantidad.Valor);
    }

    [Fact]
    public async Task La_api_devuelve_problem_details_tipado_para_recursos_y_reglas_de_negocio()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        using var respuestaNoEncontrada = await cliente.GetAsync($"/api/ventas/{Guid.NewGuid()}");
        var problemaNoEncontrada = await respuestaNoEncontrada.Content.ReadFromJsonAsync<ProblemaRespuesta>();

        Assert.Equal(HttpStatusCode.NotFound, respuestaNoEncontrada.StatusCode);
        Assert.NotNull(problemaNoEncontrada);
        Assert.Equal("Venta.NoEncontrada", problemaNoEncontrada.Codigo);
        Assert.Equal("NoEncontrado", problemaNoEncontrada.Categoria);

        var venta = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 12 });

        using var respuestaMotivoInvalido = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/cancelar",
            new { motivoCancelacion = " " });
        var problemaMotivoInvalido = await respuestaMotivoInvalido.Content.ReadFromJsonAsync<ProblemaRespuesta>();

        Assert.Equal(HttpStatusCode.BadRequest, respuestaMotivoInvalido.StatusCode);
        Assert.NotNull(problemaMotivoInvalido);
        Assert.Equal("Venta.MotivoCancelacionInvalido", problemaMotivoInvalido.Codigo);
        Assert.Equal("Validacion", problemaMotivoInvalido.Categoria);
    }

    [Fact]
    public async Task Producto_actualizado_e_inactivo_no_puede_agregarse_a_una_venta()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        var producto = await CrearAsync(
            cliente,
            "/api/productos",
            new { nombre = "Producto de catálogo", precio = 5m });

        using var respuestaActualizar = await cliente.PutAsJsonAsync(
            $"/api/productos/{producto.Id}",
            new { nombre = "Producto actualizado", precio = 7.25m });
        var productoActualizado = await respuestaActualizar.Content.ReadFromJsonAsync<ProductoRespuesta>();

        Assert.Equal(HttpStatusCode.OK, respuestaActualizar.StatusCode); 
        Assert.NotNull(productoActualizado);
        Assert.Equal("Producto actualizado", productoActualizado.Nombre);
        Assert.Equal(7.25m, productoActualizado.PrecioActual);

        using var respuestaEstadoIncompleto = await cliente.PatchAsJsonAsync(
            $"/api/productos/{producto.Id}/estado",
            new { });
        var problemaEstadoIncompleto = await respuestaEstadoIncompleto.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();

        Assert.Equal(HttpStatusCode.BadRequest, respuestaEstadoIncompleto.StatusCode);
        Assert.NotNull(problemaEstadoIncompleto);
        Assert.Equal("Producto.EstaActivoRequerido", problemaEstadoIncompleto.Codigo);
        Assert.Equal("Validacion", problemaEstadoIncompleto.Categoria);

        using var respuestaProductoActivo = await cliente.GetAsync($"/api/productos/{producto.Id}");
        var productoActivo = await respuestaProductoActivo.Content.ReadFromJsonAsync<ProductoRespuesta>();

        Assert.Equal(HttpStatusCode.OK, respuestaProductoActivo.StatusCode);
        Assert.NotNull(productoActivo);
        Assert.True(productoActivo.EstaActivo);

        using var respuestaDesactivar = await cliente.PatchAsJsonAsync(
            $"/api/productos/{producto.Id}/estado",
            new { estaActivo = false });

        Assert.Equal(HttpStatusCode.OK, respuestaDesactivar.StatusCode);

        var venta = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 13 });

        using var respuestaAgregar = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/detalles",
            new { productoMenuId = producto.Id, cantidad = 1 });
        var problema = await respuestaAgregar.Content.ReadFromJsonAsync<ProblemaRespuesta>();

        Assert.Equal((HttpStatusCode)422, respuestaAgregar.StatusCode);
        Assert.NotNull(problema);
        Assert.Equal("Venta.ProductoInactivo", problema.Codigo);
        Assert.Equal("ReglaNegocio", problema.Categoria);
    }

    [Fact]
    public async Task Crear_venta_desde_base_vacia_persiste_el_evento_en_outbox_transaccional()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        var venta = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 14 });

        var cadenaConexion = await aplicacion.ObtenerCadenaConexionAsync("restauranteventas");
        Assert.False(string.IsNullOrWhiteSpace(cadenaConexion));

        var opciones = new DbContextOptionsBuilder<RestauranteVentasDbContext>()
            .UseNpgsql(cadenaConexion)
            .Options;

        await using var contexto = new RestauranteVentasDbContext(opciones);
        var mensaje = await contexto.MensajesOutbox
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.TipoEvento.EndsWith("VentaCreadaEventoDominio"));

        Assert.NotNull(mensaje);
        Assert.Contains(venta.Id.ToString(), mensaje.Contenido, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Guid.Empty, mensaje.Id);
        Assert.Equal(DateTimeKind.Utc, mensaje.OcurridoEnUtc.Kind);
    }

    [Fact]
    public async Task Intentar_pagar_dos_veces_retorna_conflicto_http()
    {
        await using var aplicacion = await CrearAplicacionAsync();
        using var cliente = aplicacion.CreateHttpClient("api");

        var producto = await CrearAsync(
            cliente,
            "/api/productos",
            new { nombre = "Producto para pago", precio = 4m });
        var venta = await CrearAsync(
            cliente,
            "/api/ventas",
            new { clienteId = (Guid?)null, numeroMesa = 15 });

        using var respuestaAgregar = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/detalles",
            new { productoMenuId = producto.Id, cantidad = 1 });
        Assert.Equal(HttpStatusCode.OK, respuestaAgregar.StatusCode);

        using var primerPago = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/pagar",
            new { metodoPago = "Efectivo" });
        Assert.Equal(HttpStatusCode.OK, primerPago.StatusCode);

        using var segundoPago = await cliente.PostAsJsonAsync(
            $"/api/ventas/{venta.Id}/pagar",
            new { metodoPago = "Efectivo" });
        var problema = await segundoPago.Content.ReadFromJsonAsync<ProblemaRespuesta>();

        Assert.Equal(HttpStatusCode.Conflict, segundoPago.StatusCode);
        Assert.NotNull(problema);
        Assert.Equal("Venta.YaPagada", problema.Codigo);
        Assert.Equal("Conflicto", problema.Categoria);
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

    private sealed record VentaRespuesta(
        Guid Id,
        string Estado,
        decimal? Total,
        DateTime? FechaCancelacionUtc,
        string? MotivoCancelacion);

    private sealed record ProductoRespuesta(Guid Id, string Nombre, decimal PrecioActual, bool EstaActivo);

    private sealed record ProblemaRespuesta(string? Title, int? Status, string? Codigo, string? Categoria);

    private sealed class AplicacionIniciada(DistributedApplication aplicacion) : IAsyncDisposable
    {
        public HttpClient CreateHttpClient(string nombreRecurso) => aplicacion.CreateHttpClient(nombreRecurso);

        public ValueTask<string?> ObtenerCadenaConexionAsync(
            string nombreRecurso,
            CancellationToken cancellationToken = default) =>
            aplicacion.GetConnectionStringAsync(nombreRecurso, cancellationToken);

        public ValueTask DisposeAsync() => aplicacion.DisposeAsync();
    }
}
