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
    private const string MotivoCancelacion = "El cliente solicitó cancelar la comanda.";

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
        Assert.NotEqual(Guid.Empty, evento.EventoId);
        Assert.Equal(ventaId, evento.VentaId);
        Assert.Equal(FechaCreacion, evento.FechaUtc);
        Assert.Equal(FechaCreacion, evento.FechaCreacionUtc);
        Assert.Equal(FechaCreacion, evento.OcurridoEnUtc);
        Assert.Null(venta.FechaPagoUtc);
        Assert.Null(venta.FechaCancelacionUtc);
        Assert.Null(venta.MotivoCancelacion);
    }

    [Fact]
    public void Puede_existir_sin_cliente_y_sin_mesa()
    {
        var venta = Venta.Crear(Guid.NewGuid(), null, null, FechaCreacion).Valor!;

        Assert.Null(venta.ClienteId);
        Assert.Null(venta.Mesa);
    }

    [Fact]
    public void Crear_rechaza_identificador_vacio()
    {
        var resultado = Venta.Crear(Guid.Empty, null, null, FechaCreacion);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.IdInvalido.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void Crear_rechaza_cliente_vacio_cuando_se_especifica()
    {
        var resultado = Venta.Crear(Guid.NewGuid(), Guid.Empty, null, FechaCreacion);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.ClienteIdInvalido.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void Crear_con_cliente_valido_conserva_su_identificador()
    {
        var clienteId = Guid.NewGuid();

        var resultado = Venta.Crear(Guid.NewGuid(), clienteId, null, FechaCreacion);

        Assert.True(resultado.EsExito);
        Assert.Equal(clienteId, resultado.Valor!.ClienteId);
    }

    [Theory]
    [MemberData(nameof(FechasNoUtcONoInicializadas))]
    public void Crear_rechaza_fecha_de_creacion_no_utc_o_no_inicializada(DateTime fechaInvalida)
    {
        var resultado = Venta.Crear(Guid.NewGuid(), null, null, fechaInvalida);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.FechaCreacionInvalida.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void Agregar_producto_crea_detalle_con_nombre_y_precio_historico()
    {
        var venta = CrearVentaAbierta();
        var producto = CrearProducto("Ensalada", 8m);

        venta.AgregarProducto(producto, Cantidad.Crear(2).Valor!);

        producto.CambiarNombre(NombreProducto.Crear("Ensalada César").Valor!);
        producto.ActualizarPrecio(Dinero.Crear(10m).Valor!);

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
    public void Agregar_producto_rechaza_argumentos_nulos()
    {
        var venta = CrearVentaAbierta();
        var cantidad = Cantidad.Crear(1).Valor!;

        var productoNulo = venta.AgregarProducto(null, cantidad);
        var cantidadNula = venta.AgregarProducto(CrearProducto("Plato", 12m), null);

        Assert.False(productoNulo.EsExito);
        Assert.Equal(ErroresVenta.ProductoInvalido.Codigo, productoNulo.Error!.Codigo);
        Assert.False(cantidadNula.EsExito);
        Assert.Equal(ErroresVenta.CantidadInvalida.Codigo, cantidadNula.Error!.Codigo);
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
        Assert.Null(venta.FechaCancelacionUtc);
        Assert.Null(venta.MotivoCancelacion);

        var evento = Assert.Single(venta.Eventos.OfType<VentaPagadaEventoDominio>());
        Assert.NotEqual(Guid.Empty, evento.EventoId);
        Assert.Equal(venta.Id, evento.VentaId);
        Assert.Equal(12m, evento.Total.Monto);
        Assert.Equal(Dinero.MonedaUsd, evento.Total.Moneda);
        Assert.Equal(MetodoPago.Tarjeta, evento.MetodoPago);
        Assert.Equal(FechaPago, evento.FechaPagoUtc);
        Assert.Equal(FechaPago, evento.OcurridoEnUtc);
        Assert.Equal(FechaPago, evento.FechaUtc);
    }

    [Fact]
    public void Pagar_rechaza_metodo_de_pago_no_definido()
    {
        var venta = CrearVentaConProducto();

        var resultado = venta.Pagar((MetodoPago)99, FechaPago);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.MetodoPagoInvalido.Codigo, resultado.Error!.Codigo);
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

    [Theory]
    [MemberData(nameof(FechasNoUtcONoInicializadas))]
    public void Pagar_rechaza_fecha_no_utc_o_no_inicializada_sin_modificar_la_venta(DateTime fechaInvalida)
    {
        var venta = CrearVentaConProducto();
        var eventosAntes = venta.Eventos.Count;

        var resultado = venta.Pagar(MetodoPago.Efectivo, fechaInvalida);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.FechaPagoInvalida.Codigo, resultado.Error!.Codigo);
        Assert.Equal(EstadoVenta.Abierta, venta.Estado);
        Assert.Null(venta.FechaPagoUtc);
        Assert.Null(venta.MetodoPago);
        Assert.Equal(eventosAntes, venta.Eventos.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Pagar_rechaza_fecha_anterior_o_igual_a_la_creacion(int minutosDesdeCreacion)
    {
        var venta = CrearVentaConProducto();
        var fechaPagoInvalida = FechaCreacion.AddMinutes(minutosDesdeCreacion);

        var resultado = venta.Pagar(MetodoPago.Efectivo, fechaPagoInvalida);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.FechaPagoNoPosteriorACreacion.Codigo, resultado.Error!.Codigo);
        Assert.Equal(EstadoVenta.Abierta, venta.Estado);
        Assert.Null(venta.FechaPagoUtc);
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

        var resultado = venta.Cancelar(FechaCancelacion, $"  {MotivoCancelacion}  ");

        Assert.True(resultado.EsExito);
        Assert.Equal(EstadoVenta.Cancelada, venta.Estado);
        Assert.Equal(FechaCancelacion, venta.FechaCancelacionUtc);
        Assert.Equal(MotivoCancelacion, venta.MotivoCancelacion);
        Assert.Null(venta.FechaPagoUtc);
        Assert.Null(venta.MetodoPago);

        var evento = Assert.Single(venta.Eventos.OfType<VentaCanceladaEventoDominio>());
        Assert.NotEqual(Guid.Empty, evento.EventoId);
        Assert.Equal(venta.Id, evento.VentaId);
        Assert.Equal(MotivoCancelacion, evento.MotivoCancelacion);
        Assert.Equal(FechaCancelacion, evento.FechaCancelacionUtc);
        Assert.Equal(FechaCancelacion, evento.OcurridoEnUtc);
        Assert.Equal(FechaCancelacion, evento.FechaUtc);
    }

    [Fact]
    public void No_permite_pagar_venta_cancelada()
    {
        var venta = CrearVentaConProducto();
        venta.Cancelar(FechaCancelacion, MotivoCancelacion);

        var resultado = venta.Pagar(MetodoPago.Efectivo, FechaPago);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.YaCancelada.Codigo, resultado.Error!.Codigo);
    }

    [Fact]
    public void No_permite_modificar_venta_cancelada()
    {
        var venta = CrearVentaConProducto();
        var detalleId = venta.Detalles.Single().Id;
        venta.Cancelar(FechaCancelacion, MotivoCancelacion);

        var agregar = venta.AgregarProducto(CrearProducto("Otro", 5m), Cantidad.Crear(1).Valor!);
        var cambiar = venta.CambiarCantidad(detalleId, Cantidad.Crear(2).Valor!);
        var eliminar = venta.EliminarDetalle(detalleId);

        Assert.False(agregar.EsExito);
        Assert.False(cambiar.EsExito);
        Assert.False(eliminar.EsExito);
        Assert.Equal(ErroresVenta.YaCancelada.Codigo, agregar.Error!.Codigo);
        Assert.Equal(ErroresVenta.YaCancelada.Codigo, cambiar.Error!.Codigo);
        Assert.Equal(ErroresVenta.YaCancelada.Codigo, eliminar.Error!.Codigo);
    }

    [Fact]
    public void No_permite_cancelar_venta_pagada()
    {
        var venta = CrearVentaConProducto();
        venta.Pagar(MetodoPago.Efectivo, FechaPago);

        var resultado = venta.Cancelar(FechaCancelacion, MotivoCancelacion);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.YaPagada.Codigo, resultado.Error!.Codigo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancelar_exige_motivo_de_auditoria(string? motivoCancelacion)
    {
        var venta = CrearVentaConProducto();
        var eventosAntes = venta.Eventos.Count;

        var resultado = venta.Cancelar(FechaCancelacion, motivoCancelacion);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.MotivoCancelacionInvalido.Codigo, resultado.Error!.Codigo);
        Assert.Equal(EstadoVenta.Abierta, venta.Estado);
        Assert.Null(venta.FechaCancelacionUtc);
        Assert.Null(venta.MotivoCancelacion);
        Assert.Equal(eventosAntes, venta.Eventos.Count);
    }

    [Fact]
    public void Cancelar_rechaza_motivo_mas_largo_que_el_maximo_permitido()
    {
        var venta = CrearVentaConProducto();
        var motivoDemasiadoLargo = new string('x', Venta.LongitudMaximaMotivoCancelacion + 1);

        var resultado = venta.Cancelar(FechaCancelacion, motivoDemasiadoLargo);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.MotivoCancelacionInvalido.Codigo, resultado.Error!.Codigo);
        Assert.Equal(EstadoVenta.Abierta, venta.Estado);
    }

    [Fact]
    public void Cancelar_acepta_motivo_con_la_longitud_maxima_permitida()
    {
        var venta = CrearVentaConProducto();
        var motivoMaximo = new string('x', Venta.LongitudMaximaMotivoCancelacion);

        var resultado = venta.Cancelar(FechaCancelacion, motivoMaximo);

        Assert.True(resultado.EsExito);
        Assert.Equal(motivoMaximo, venta.MotivoCancelacion);
    }

    [Theory]
    [MemberData(nameof(FechasNoUtcONoInicializadas))]
    public void Cancelar_rechaza_fecha_no_utc_o_no_inicializada_sin_modificar_la_venta(DateTime fechaInvalida)
    {
        var venta = CrearVentaConProducto();
        var eventosAntes = venta.Eventos.Count;

        var resultado = venta.Cancelar(fechaInvalida, MotivoCancelacion);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.FechaCancelacionInvalida.Codigo, resultado.Error!.Codigo);
        Assert.Equal(EstadoVenta.Abierta, venta.Estado);
        Assert.Null(venta.FechaCancelacionUtc);
        Assert.Null(venta.MotivoCancelacion);
        Assert.Equal(eventosAntes, venta.Eventos.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Cancelar_rechaza_fecha_anterior_o_igual_a_la_creacion(int minutosDesdeCreacion)
    {
        var venta = CrearVentaConProducto();
        var fechaCancelacionInvalida = FechaCreacion.AddMinutes(minutosDesdeCreacion);

        var resultado = venta.Cancelar(fechaCancelacionInvalida, MotivoCancelacion);

        Assert.False(resultado.EsExito);
        Assert.Equal(ErroresVenta.FechaCancelacionNoPosteriorACreacion.Codigo, resultado.Error!.Codigo);
        Assert.Equal(EstadoVenta.Abierta, venta.Estado);
        Assert.Null(venta.FechaCancelacionUtc);
    }

    [Fact]
    public void Eventos_de_transicion_tienen_identificadores_distintos()
    {
        var venta = CrearVentaConProducto();

        venta.Pagar(MetodoPago.Efectivo, FechaPago);

        Assert.Equal(venta.Eventos.Count, venta.Eventos.Select(evento => evento.EventoId).Distinct().Count());
    }

    public static IEnumerable<object[]> FechasNoUtcONoInicializadas()
    {
        yield return [default(DateTime)];
        yield return [new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc)];
        yield return [new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Unspecified)];
        yield return [new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Local)];
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
