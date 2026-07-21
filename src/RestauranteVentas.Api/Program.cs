using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Api.Contratos;
using RestauranteVentas.Api.Infraestructura;
using RestauranteVentas.Api.Respuestas;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Productos.Commands.CrearProductoMenu;
using RestauranteVentas.Aplicacion.Productos.Queries.ObtenerProductoMenuPorId;
using RestauranteVentas.Aplicacion.Ventas.Commands.AgregarProductoVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.CambiarCantidadDetalleVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.CancelarVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.CrearVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.EliminarDetalleVenta;
using RestauranteVentas.Aplicacion.Ventas.Commands.PagarVenta;
using RestauranteVentas.Aplicacion.Ventas.Queries.ObtenerVentaPorId;
using RestauranteVentas.Infrastructure;
using RestauranteVentas.Infrastructure.Persistencia;

var constructor = WebApplication.CreateBuilder(args);

var cadenaConexion = constructor.Configuration.GetConnectionString("restauranteventas")
    ?? throw new InvalidOperationException("La cadena de conexión 'restauranteventas' no está configurada.");

constructor.Services.AgregarInfraestructura(cadenaConexion);
constructor.Services.AddScoped<IReloj, RelojSistema>();
constructor.Services.AddScoped<IGeneradorIdentidad, GeneradorIdentidadGuid>();

constructor.Services.AddScoped<IComandoHandler<CrearProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>>, CrearProductoMenuHandler>();
constructor.Services.AddScoped<IConsultaHandler<ObtenerProductoMenuPorIdConsulta, ResultadoAplicacion<ProductoMenuDto>>, ObtenerProductoMenuPorIdHandler>();
constructor.Services.AddScoped<IComandoHandler<CrearVentaComando, ResultadoAplicacion<VentaDto>>, CrearVentaHandler>();
constructor.Services.AddScoped<IConsultaHandler<ObtenerVentaPorIdConsulta, ResultadoAplicacion<VentaDto>>, ObtenerVentaPorIdHandler>();
constructor.Services.AddScoped<IComandoHandler<AgregarProductoVentaComando, ResultadoAplicacion<VentaDto>>, AgregarProductoVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<CambiarCantidadDetalleVentaComando, ResultadoAplicacion<VentaDto>>, CambiarCantidadDetalleVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<EliminarDetalleVentaComando, ResultadoAplicacion<VentaDto>>, EliminarDetalleVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<PagarVentaComando, ResultadoAplicacion<VentaDto>>, PagarVentaHandler>();
constructor.Services.AddScoped<IComandoHandler<CancelarVentaComando, ResultadoAplicacion<VentaDto>>, CancelarVentaHandler>();

constructor.Services.AddHealthChecks();

var aplicacion = constructor.Build();

await using (var alcance = aplicacion.Services.CreateAsyncScope())
{
    var contexto = alcance.ServiceProvider.GetRequiredService<RestauranteVentasDbContext>();
    await contexto.Database.MigrateAsync();
}

aplicacion.MapHealthChecks("/health");

aplicacion.MapPost("/api/productos", async (
    CrearProductoSolicitud solicitud,
    IComandoHandler<CrearProductoMenuComando, ResultadoAplicacion<ProductoMenuDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new CrearProductoMenuComando(solicitud.Nombre, solicitud.Precio),
        cancellationToken);

    return ResultadosHttp.Desde(resultado, resultado.EsExito ? $"/api/productos/{resultado.Valor!.Id}" : null);
});

aplicacion.MapGet("/api/productos/{productoMenuId:guid}", async (
    Guid productoMenuId,
    IConsultaHandler<ObtenerProductoMenuPorIdConsulta, ResultadoAplicacion<ProductoMenuDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new ObtenerProductoMenuPorIdConsulta(productoMenuId),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
});

aplicacion.MapPost("/api/ventas", async (
    CrearVentaSolicitud solicitud,
    IComandoHandler<CrearVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new CrearVentaComando(solicitud.ClienteId, solicitud.NumeroMesa),
        cancellationToken);

    return ResultadosHttp.Desde(resultado, resultado.EsExito ? $"/api/ventas/{resultado.Valor!.Id}" : null);
});

aplicacion.MapGet("/api/ventas/{ventaId:guid}", async (
    Guid ventaId,
    IConsultaHandler<ObtenerVentaPorIdConsulta, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new ObtenerVentaPorIdConsulta(ventaId),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
});

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
});

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
});

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
});

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
});

aplicacion.MapPost("/api/ventas/{ventaId:guid}/cancelar", async (
    Guid ventaId,
    IComandoHandler<CancelarVentaComando, ResultadoAplicacion<VentaDto>> manejador,
    CancellationToken cancellationToken) =>
{
    var resultado = await manejador.ManejarAsync(
        new CancelarVentaComando(ventaId),
        cancellationToken);

    return ResultadosHttp.Desde(resultado);
});

await aplicacion.RunAsync();

public partial class Program;
