using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RestauranteVentas.Api.Contratos;
using RestauranteVentas.Api.Respuestas;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Productos.Commands.ActualizarProductoMenu;
using RestauranteVentas.Aplicacion.Productos.Commands.CambiarEstadoProductoMenu;
using RestauranteVentas.Aplicacion.Productos.Commands.CrearProductoMenu;
using RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductos;
using RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductoMenuPorId;
using RestauranteVentas.Aplicacion.Ventas.Commands.AgregarProductoVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.CambiarCantidadDetalleVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.CancelarVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.CrearVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.EliminarDetalleVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.PagarVenta;
using RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentas;
using RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentaPorId;
using RestauranteVentas.Infrastructure;
using RestauranteVentas.Infrastructure.Persistencia;

var constructor = WebApplication.CreateBuilder(args);

constructor.AddServiceDefaults();

var cadenaConexion = constructor.Configuration.GetConnectionString("restauranteventas")
    ?? throw new InvalidOperationException("La cadena de conexión 'restauranteventas' no está configurada.");

constructor.Services.AgregarInfraestructura(cadenaConexion);

constructor.Services.AddScoped<IComandoHandler<CrearProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>>, CrearProductoMenuHandler>();
constructor.Services.AddScoped<IComandoHandler<ActualizarProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>>, ActualizarProductoMenuHandler>();
constructor.Services.AddScoped<IComandoHandler<CambiarEstadoProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>>, CambiarEstadoProductoMenuHandler>();
constructor.Services.AddScoped<IConsultaHandler<ObtenerProductosConsulta, ResultadoAplicacion<IReadOnlyCollection<ProductoMenuDto>>>, ObtenerProductosHandler>();
constructor.Services.AddScoped<IConsultaHandler<ObtenerProductoMenuPorIdConsulta, ResultadoAplicacion<ProductoMenuDto>>, ObtenerProductoMenuPorIdHandler>();
constructor.Services.AddScoped<IComandoHandler<CrearVentaComando, ResultadoAplicacion<VentaDto>>, CrearVentaHandler>();
constructor.Services.AddScoped<IConsultaHandler<ObtenerVentasConsulta, ResultadoAplicacion<IReadOnlyCollection<VentaDto>>>, ObtenerVentasHandler>();
constructor.Services.AddScoped<IConsultaHandler<ObtenerVentaPorIdConsulta, ResultadoAplicacion<VentaDto>>, ObtenerVentaPorIdHandler>();
constructor.Services.AddScoped<IComandoHandler<AgregarProductoVentaComando, ResultadoAplicacion<VentaDto>>, AgregarProductoVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<CambiarCantidadDetalleVentaComando, ResultadoAplicacion<VentaDto>>, CambiarCantidadDetalleVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<EliminarDetalleVentaComando, ResultadoAplicacion<VentaDto>>, EliminarDetalleVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<PagarVentaComando, ResultadoAplicacion<VentaDto>>, PagarVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<CancelarVentaComando, ResultadoAplicacion<VentaDto>>, CancelarVentaHandler>();

constructor.Services.AddEndpointsApiExplorer();
constructor.Services.AddSwaggerGen();
constructor.Services.AddProblemDetails();
constructor.Services.AddExceptionHandler<ManejadorExcepciones>();
var aplicacion = constructor.Build();

aplicacion.UseExceptionHandler();

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseSwagger();
    aplicacion.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurante Ventas API v1");
    });
}

if (aplicacion.Environment.IsDevelopment())
{
    await using var alcance = aplicacion.Services.CreateAsyncScope();
    var contexto = alcance.ServiceProvider.GetRequiredService<RestauranteVentasDbContext>();
    await contexto.Database.MigrateAsync();
}

aplicacion.MapDefaultEndpoints();

aplicacion.MapPost("/api/productos", async (
    CrearProductoSolicitud solicitud,
    IComandoHandler<CrearProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new CrearProductoMenuComando(solicitud.Nombre, solicitud.Precio),
        cancellationToken);

    return ResultadosHttp.Desde(resultado, resultado.EsExito ? $"/api/productos/{resultado.Valor!.Id}" : null);
}).WithTags("Productos");

aplicacion.MapGet("/api/productos", async (
    IConsultaHandler<ObtenerProductosConsulta, ResultadoAplicacion<IReadOnlyCollection<ProductoMenuDto>>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(new ObtenerProductosConsulta(), cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Productos");

aplicacion.MapPut("/api/productos/{productoMenuId:guid}", async (
    Guid productoMenuId,
    ActualizarProductoSolicitud solicitud,
    IComandoHandler<ActualizarProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new ActualizarProductoMenuComando(productoMenuId, solicitud.Nombre, solicitud.Precio),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Productos");

aplicacion.MapPatch("/api/productos/{productoMenuId:guid}/estado", async (
    Guid productoMenuId,
    CambiarEstadoProductoSolicitud solicitud,
    IComandoHandler<CambiarEstadoProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>> manejador,
    CancellationToken cancellationToken) =>
{
    if (!solicitud.EstaActivo.HasValue)
    {
        return ResultadosHttp.Desde(ResultadoAplicacion<ProductoMenuDto>.Fallo(
            "Producto.EstaActivoRequerido",
            "El campo estaActivo es obligatorio."));
    }

    var resultado = await manejador.ManejarAsync(
        new CambiarEstadoProductoMenuComando(productoMenuId, solicitud.EstaActivo.Value),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Productos");

aplicacion.MapGet("/api/productos/{productoMenuId:guid}", async (
    Guid productoMenuId,
    IConsultaHandler<ObtenerProductoMenuPorIdConsulta, ResultadoAplicacion<ProductoMenuDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new ObtenerProductoMenuPorIdConsulta(productoMenuId),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Productos");

aplicacion.MapPost("/api/ventas", async (
    CrearVentaSolicitud solicitud,
    IComandoHandler<CrearVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new CrearVentaComando(solicitud.ClienteId, solicitud.NumeroMesa),
        cancellationToken);

    return ResultadosHttp.Desde(resultado, resultado.EsExito ? $"/api/ventas/{resultado.Valor!.Id}" : null);
}).WithTags("Ventas");

aplicacion.MapGet("/api/ventas", async (
    IConsultaHandler<ObtenerVentasConsulta, ResultadoAplicacion<IReadOnlyCollection<VentaDto>>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(new ObtenerVentasConsulta(), cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Ventas");

aplicacion.MapGet("/api/ventas/{ventaId:guid}", async (
    Guid ventaId,
    IConsultaHandler<ObtenerVentaPorIdConsulta, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new ObtenerVentaPorIdConsulta(ventaId),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Ventas");

aplicacion.MapPost("/api/ventas/{ventaId:guid}/detalles", async (
    Guid ventaId,
    AgregarDetalleVentaSolicitud solicitud,
    IComandoHandler<AgregarProductoVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new AgregarProductoVentaComando(ventaId, solicitud.ProductoMenuId, solicitud.Cantidad),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Ventas");

aplicacion.MapPut("/api/ventas/{ventaId:guid}/detalles/{detalleId:guid}", async (
    Guid ventaId,
    Guid detalleId,
    CambiarCantidadDetalleVentaSolicitud solicitud,
    IComandoHandler<CambiarCantidadDetalleVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new CambiarCantidadDetalleVentaComando(ventaId, detalleId, solicitud.NuevaCantidad),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Ventas");

aplicacion.MapDelete("/api/ventas/{ventaId:guid}/detalles/{detalleId:guid}", async (
    Guid ventaId,
    Guid detalleId,
    IComandoHandler<EliminarDetalleVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new EliminarDetalleVentaComando(ventaId, detalleId),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Ventas");

aplicacion.MapPost("/api/ventas/{ventaId:guid}/pagar", async (
    Guid ventaId,
    PagarVentaSolicitud solicitud,
    IComandoHandler<PagarVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new PagarVentaComando(ventaId, solicitud.MetodoPago),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Ventas");

aplicacion.MapPost("/api/ventas/{ventaId:guid}/cancelar", async (
    Guid ventaId,
    CancelarVentaSolicitud solicitud,
    IComandoHandler<CancelarVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new CancelarVentaComando(ventaId, solicitud.MotivoCancelacion),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
}).WithTags("Ventas");

await aplicacion.RunAsync();

public partial class Program;
