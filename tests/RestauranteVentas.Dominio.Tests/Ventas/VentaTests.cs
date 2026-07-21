using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Dominio.Ventas.Eventos;

namespace RestauranteVentas.Dominio.Tests.Ventas;

public class VentaTests
{
    private static readonly DateTime FechaCreacion = new(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FechaPago = new(2026, 3, 15, 11, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FechaCancelacion = new(2026, 3, 15, 11, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Crear_venta_abierta_y_registra_evento_creada()
    {
        var ventaId = Guid.NewGuid();

        var resultado = Venta.Crear(ventaId, null, null, FechaCreacion);

        Assert.True(resultado.EsExito);
        var venta = resultado.Valor!;
        Assert.Equal(EstadoVenta.Abierta, venta.Estado);
        Assert.Single(venta.Eventos);
        Assert.IsType<VentaCreadaEventoDominio>(venta.Eventos.First());
        var evento = (VentaCreadaEventoDominio)venta.Eventos.First();
        Assert.Equal(ventaId, evento.VentaId);
        Assert.Equal(FechaCreacion, evento.FechaUtc);
    }

    [Fact]
    public void Puede_existir_sin_cliente_y_sin_mesa()
    {
        var venta = Venta.Crear(Guid.NewGuid(), null, null, FechaCreacion).Valor!;

        Assert.Null(venta.ClienteId);
        Assert.Null(venta.Mesa);
    }

    [Fact]
    public void Agregar_producto_crea_detalle_con_nombre_y_precio_historico()
    {
        var venta = CrearVentaAbierta();
        var producto = CrearProducto("Ensalada", 8m);
        producto.CambiarNombre(NombreProducto.Crear("Ensalada César").Valor!);
        producto.ActualizarPrecio(Dinero.Crear(10m).Valor!);

        var productoOriginal = CrearProducto("Ensalada", 8m);
        venta.AgregarProducto(productoOriginal, Cantidad.Crear(2).Valor!);

        var detalle = venta.Detalles.Single();
        Assert.Equal("Ensalada", detalle.NombreHistorico.Valor);
        Assert.Equal(8m, detalle.PrecioUnitarioHistorico.Monto);
    }

    [Fact]
    public void Total_se_calcula_correctamente()
    {
        var venta = CrearVentaAbierta();
        venta.AgregarProducto(CrearProducto("Item A", 10m), Cantidad.Crear(2).Valor!);
        venta.AgregarProducto(CrearProducto("Item B", 5m), Cantidad.Crear(1).Valor!);

        Assert.Equal(25m, venta.Total!.Monto);
    }

    [Fact]
    public void Cambiar_cantidad_actualiza_total()
    {
        var venta = CrearVentaAbierta();
        venta.AgregarProducto(CrearProducto("Item A", 10m), Cantidad.Crear(1).Valor!);
        var detalleId = venta.Detalles.Single().Id;

        venta.CambiarCantidad(detalleId, Cantidad.Crear(3).Valor!);

        Assert.Equal(30m, venta.Total!.Monto);
    }

    [Fact]
    public void Eliminar_detalle_actualiza_total()
    {
        var venta = CrearVentaAbierta();
        venta.AgregarProducto(CrearProducto("Item A", 10m), Cantidad.Crear(2).Valor!);
        venta.AgregarProducto(CrearProducto("Item B", 5m), Cantidad.Crear(1).Valor!);
        var detalleId = venta.Detalles.First().Id;

        venta.EliminarDetalle(detalleId);

        Assert.Single(venta.Detalles);
        Assert.Equal(5m, venta.Total!.Monto);
    }

    [Fact]
    public void No_permite_pagar_sin_detalles()
    {
        var venta = CrearVentaAbierta();

        var resultado = venta.Pagar(MetodoPago.Efectivo, FechaPago);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.SinDetalles.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void Pagar_cambia_estado_registra_fecha_metodo_y_evento()
    {
        var venta = CrearVentaConProducto();

        var resultado = venta.Pagar(MetodoPago.Tarjeta, FechaPago);

        Assert.True(resultado.EsExito);
        Assert.Equal(EstadoVenta.Pagada, venta.Estado);
        Assert.Equal(MetodoPago.Tarjeta, venta.MetodoPago);
        Assert.Equal(FechaPago, venta.FechaPagoUtc);
        Assert.Contains(venta.Eventos, e => e is VentaPagadaEventoDominio);
    }

    [Fact]
    public void No_permite_pagar_dos_veces()
    {
        var venta = CrearVentaConProducto();
        venta.Pagar(MetodoPago.Efectivo, FechaPago);

        var resultado = venta.Pagar(MetodoPago.Tarjeta, FechaPago);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.YaPagada.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void No_permite_modificar_venta_pagada()
    {
        var venta = CrearVentaConProducto();
        venta.Pagar(MetodoPago.Efectivo, FechaPago);
        var detalleId = venta.Detalles.Single().Id;

        var agregar = venta.AgregarProducto(CrearProducto("Otro", 5m), Cantidad.Crear(1).Valor!);
        var cambiar = venta.CambiarCantidad(detalleId, Cantidad.Crear(2).Valor!);
        var eliminar = venta.EliminarDetalle(detalleId);

        Assert.False(agregar.EsExito);
        Assert.False(cambiar.EsExito);
        Assert.False(eliminar.EsExito);
        Assert.Equal(ErroresVenta.YaPagada.Codigo, agregar.Error!.Codigo);
    }

    [Fact]
    public void Cancelar_cambia_estado_y_registra_evento()
    {
        var venta = CrearVentaConProducto();

        var resultado = venta.Cancelar(FechaCancelacion);

        Assert.True(resultado.EsExito);
        Assert.Equal(EstadoVenta.Cancelada, venta.Estado);
        Assert.Contains(venta.Eventos, e => e is VentaCanceladaEventoDominio);
    }

    [Fact]
    public void No_permite_pagar_venta_cancelada()
    {
        var venta = CrearVentaConProducto();
        venta.Cancelar(FechaCancelacion);

        var resultado = venta.Pagar(MetodoPago.Efectivo, FechaPago);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.YaCancelada.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void No_permite_cancelar_venta_pagada()
    {
        var venta = CrearVentaConProducto();
        venta.Pagar(MetodoPago.Efectivo, FechaPago);

        var resultado = venta.Cancelar(FechaCancelacion);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.YaPagada.Codigo, resultado.Error!.Codigo);
    }

    private static Venta CrearVentaAbierta() =>
        Venta.Crear(Guid.NewGuid(), null, null, FechaCreacion).Valor!;

    private static Venta CrearVentaConProducto()
    {
        var venta = CrearVentaAbierta();
        venta.AgregarProducto(CrearProducto("Plato", 12m), Cantidad.Crear(1).Valor!);
        return venta;
    }

    private static ProductoMenu CrearProducto(string nombre, decimal precio) =>
        ProductoMenu.Crear(
            Guid.NewGuid(),
            NombreProducto.Crear(nombre).Valor!,
            Dinero.Crear(precio).Valor!).Valor!;
}
