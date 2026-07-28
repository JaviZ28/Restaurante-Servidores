using Microsoft.EntityFrameworkCore;
using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Aplicacion.Ventas.Queries;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Ventas;
using System.Linq.Expressions;

namespace RestauranteVentas.Infrastructure.Persistencia.Lecturas;

/// <summary>
/// Proyección CQRS de una venta. Lee exclusivamente las columnas necesarias y
/// deshabilita el tracking para no reconstruir el agregado de escritura.
/// </summary>
public sealed class LecturaVentaEfCore(RestauranteVentasDbContext contexto) : IVentaLectura
{
    private static readonly Expression<Func<Venta, VentaProyeccion>> CrearProyeccion = venta =>
        new VentaProyeccion(
            venta.Id,
            venta.ClienteId,
            venta.Mesa,
            venta.Estado,
            venta.FechaCreacionUtc,
            venta.FechaPagoUtc,
            venta.FechaCancelacionUtc,
            venta.MetodoPago,
            venta.MotivoCancelacion,
            venta.Detalles
                .Select(detalle => new DetalleVentaProyeccion(
                    detalle.Id,
                    detalle.ProductoMenuId,
                    detalle.NombreHistorico,
                    detalle.PrecioUnitarioHistorico,
                    detalle.Cantidad))
                .ToList());

    public async Task<IReadOnlyCollection<VentaDto>> ObtenerTodasAsync(
        CancellationToken cancellationToken = default)
    {
        var proyecciones = await contexto.Ventas
            .AsNoTracking()
            .OrderByDescending(venta => venta.FechaCreacionUtc)
            .ThenByDescending(venta => venta.Id)
            .Select(CrearProyeccion)
            .ToListAsync(cancellationToken);

        return proyecciones
            .Select(AVentaDto)
            .ToList();
    }

    public async Task<VentaDto?> ObtenerPorIdAsync(
        Guid ventaId,
        CancellationToken cancellationToken = default)
    {
        var proyeccion = await contexto.Ventas
            .AsNoTracking()
            .Where(venta => venta.Id == ventaId)
            .Select(CrearProyeccion)
            .SingleOrDefaultAsync(cancellationToken);

        return proyeccion is null ? null : AVentaDto(proyeccion);
    }

    private static VentaDto AVentaDto(VentaProyeccion proyeccion)
    {
        var detalles = proyeccion.Detalles
            .Select(detalle => new DetalleVentaDto(
                detalle.Id,
                detalle.ProductoMenuId,
                detalle.NombreHistorico.Valor,
                detalle.PrecioUnitarioHistorico.Monto,
                detalle.PrecioUnitarioHistorico.Moneda,
                detalle.Cantidad.Valor,
                detalle.PrecioUnitarioHistorico.Monto * detalle.Cantidad.Valor))
            .ToList();

        decimal? total = detalles.Count == 0
            ? null
            : detalles.Sum(detalle => detalle.Subtotal);

        return new VentaDto(
            proyeccion.Id,
            proyeccion.ClienteId,
            proyeccion.Mesa?.Valor,
            proyeccion.Estado.ToString(),
            proyeccion.FechaCreacionUtc,
            proyeccion.FechaPagoUtc,
            proyeccion.FechaCancelacionUtc,
            proyeccion.MetodoPago?.ToString(),
            proyeccion.MotivoCancelacion,
            total,
            total is null ? null : Dinero.MonedaUsd,
            detalles);
    }

    private sealed record VentaProyeccion(
        Guid Id,
        Guid? ClienteId,
        NumeroMesa? Mesa,
        EstadoVenta Estado,
        DateTime FechaCreacionUtc,
        DateTime? FechaPagoUtc,
        DateTime? FechaCancelacionUtc,
        MetodoPago? MetodoPago,
        string? MotivoCancelacion,
        List<DetalleVentaProyeccion> Detalles);

    private sealed record DetalleVentaProyeccion(
        Guid Id,
        Guid ProductoMenuId,
        NombreProducto NombreHistorico,
        Dinero PrecioUnitarioHistorico,
        Cantidad Cantidad);
}
