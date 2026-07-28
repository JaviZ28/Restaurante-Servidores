using RestauranteVentas.Dominio.Abstracciones;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas.Eventos;

namespace RestauranteVentas.Dominio.Ventas;

/// <summary>
/// Agregado que representa la comanda mientras está abierta y el resultado del
/// cobro cuando pasa a estado <see cref="EstadoVenta.Pagada"/>.
/// </summary>
public sealed class Venta : Entidad
{
    public const int LongitudMaximaMotivoCancelacion = 500;

    public Guid? ClienteId { get; private set; }
    public NumeroMesa? Mesa { get; private set; }
    public EstadoVenta Estado { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }
    public DateTime? FechaPagoUtc { get; private set; }
    public MetodoPago? MetodoPago { get; private set; }
    public DateTime? FechaCancelacionUtc { get; private set; }
    public string? MotivoCancelacion { get; private set; }
    /// <summary>
    /// Revisión interna del agregado. Cambia con cada mutación exitosa para
    /// que una modificación de un detalle también actualice la raíz y active
    /// el control optimista de concurrencia de la venta.
    /// </summary>
    public Guid Revision { get; private set; }

    private readonly List<DetalleVenta> _detalles = [];

    public IReadOnlyCollection<DetalleVenta> Detalles => _detalles.AsReadOnly();

    public Dinero? Total => CalcularTotal();

    private Venta(
        Guid id,
        Guid? clienteId,
        NumeroMesa? mesa,
        DateTime fechaCreacionUtc)
        : base(id)
    {
        ClienteId = clienteId;
        Mesa = mesa;
        Estado = EstadoVenta.Abierta;
        FechaCreacionUtc = fechaCreacionUtc;
        Revision = Guid.NewGuid();
    }

    public static Resultado<Venta> Crear(
        Guid id,
        Guid? clienteId,
        NumeroMesa? mesa,
        DateTime fechaCreacionUtc)
    {
        if (id == Guid.Empty)
        {
            return Resultado<Venta>.Fallo(ErroresVenta.IdInvalido);
        }

        if (clienteId.HasValue && clienteId.Value == Guid.Empty)
        {
            return Resultado<Venta>.Fallo(ErroresVenta.ClienteIdInvalido);
        }

        if (!EsFechaUtcValida(fechaCreacionUtc))
        {
            return Resultado<Venta>.Fallo(ErroresVenta.FechaCreacionInvalida);
        }

        var venta = new Venta(id, clienteId, mesa, fechaCreacionUtc);
        venta.RegistrarEvento(new VentaCreadaEventoDominio(id, fechaCreacionUtc));
        return Resultado<Venta>.Exito(venta);
    }

    public Resultado AgregarProducto(ProductoMenu? producto, Cantidad? cantidad)
    {
        var errorEstado = ValidarVentaAbierta();
        if (errorEstado is not null)
        {
            return Resultado.Fallo(errorEstado);
        }

        if (producto is null)
        {
            return Resultado.Fallo(ErroresVenta.ProductoInvalido);
        }

        if (cantidad is null)
        {
            return Resultado.Fallo(ErroresVenta.CantidadInvalida);
        }

        if (!producto.EstaActivo)
        {
            return Resultado.Fallo(ErroresVenta.ProductoInactivo);
        }

        var detalleId = Guid.NewGuid();
        var resultadoDetalle = DetalleVenta.Crear(detalleId, producto, cantidad);
        if (!resultadoDetalle.EsExito)
        {
            return Resultado.Fallo(resultadoDetalle.Error!);
        }

        var resultadoTotal = ValidarTotalConDetalle(resultadoDetalle.Valor!);
        if (!resultadoTotal.EsExito)
        {
            return resultadoTotal;
        }

        _detalles.Add(resultadoDetalle.Valor!);
        Tocar();
        return Resultado.Exito();
    }

    public Resultado CambiarCantidad(Guid detalleId, Cantidad? nuevaCantidad)
    {
        var errorEstado = ValidarVentaAbierta();
        if (errorEstado is not null)
        {
            return Resultado.Fallo(errorEstado);
        }

        if (nuevaCantidad is null)
        {
            return Resultado.Fallo(ErroresVenta.CantidadInvalida);
        }

        var detalle = BuscarDetalle(detalleId);
        if (detalle is null)
        {
            return Resultado.Fallo(ErroresVenta.DetalleNoEncontrado);
        }

        var resultadoTotal = ValidarTotalConCantidad(detalle, nuevaCantidad);
        if (!resultadoTotal.EsExito)
        {
            return resultadoTotal;
        }

        var resultado = detalle.CambiarCantidad(nuevaCantidad);
        if (resultado.EsExito)
        {
            Tocar();
        }

        return resultado;
    }

    public Resultado EliminarDetalle(Guid detalleId)
    {
        var errorEstado = ValidarVentaAbierta();
        if (errorEstado is not null)
        {
            return Resultado.Fallo(errorEstado);
        }

        var detalle = BuscarDetalle(detalleId);
        if (detalle is null)
        {
            return Resultado.Fallo(ErroresVenta.DetalleNoEncontrado);
        }

        _detalles.Remove(detalle);
        Tocar();
        return Resultado.Exito();
    }

    public Resultado Pagar(MetodoPago metodoPago, DateTime fechaPagoUtc)
    {
        if (Estado == EstadoVenta.Pagada)
        {
            return Resultado.Fallo(ErroresVenta.YaPagada);
        }

        if (Estado == EstadoVenta.Cancelada)
        {
            return Resultado.Fallo(ErroresVenta.YaCancelada);
        }

        if (!Enum.IsDefined(metodoPago))
        {
            return Resultado.Fallo(ErroresVenta.MetodoPagoInvalido);
        }

        if (!EsFechaUtcValida(fechaPagoUtc))
        {
            return Resultado.Fallo(ErroresVenta.FechaPagoInvalida);
        }

        if (fechaPagoUtc <= FechaCreacionUtc)
        {
            return Resultado.Fallo(ErroresVenta.FechaPagoNoPosteriorACreacion);
        }

        if (_detalles.Count == 0)
        {
            return Resultado.Fallo(ErroresVenta.SinDetalles);
        }

        var total = Total!;
        Estado = EstadoVenta.Pagada;
        MetodoPago = metodoPago;
        FechaPagoUtc = fechaPagoUtc;
        Tocar();
        RegistrarEvento(new VentaPagadaEventoDominio(Id, total, metodoPago, fechaPagoUtc));
        return Resultado.Exito();
    }

    /// <summary>
    /// Cancela una comanda abierta y conserva el motivo como parte de su
    /// auditoría de negocio.
    /// </summary>
    public Resultado Cancelar(DateTime fechaCancelacionUtc, string? motivoCancelacion)
    {
        if (Estado == EstadoVenta.Pagada)
        {
            return Resultado.Fallo(ErroresVenta.YaPagada);
        }

        if (Estado == EstadoVenta.Cancelada)
        {
            return Resultado.Fallo(ErroresVenta.YaCancelada);
        }

        if (!EsFechaUtcValida(fechaCancelacionUtc))
        {
            return Resultado.Fallo(ErroresVenta.FechaCancelacionInvalida);
        }

        if (fechaCancelacionUtc <= FechaCreacionUtc)
        {
            return Resultado.Fallo(ErroresVenta.FechaCancelacionNoPosteriorACreacion);
        }

        var resultadoMotivo = NormalizarMotivoCancelacion(motivoCancelacion);
        if (!resultadoMotivo.EsExito)
        {
            return Resultado.Fallo(resultadoMotivo.Error!);
        }

        var motivoNormalizado = resultadoMotivo.Valor!;
        Estado = EstadoVenta.Cancelada;
        FechaCancelacionUtc = fechaCancelacionUtc;
        MotivoCancelacion = motivoNormalizado;
        Tocar();
        RegistrarEvento(new VentaCanceladaEventoDominio(Id, motivoNormalizado, fechaCancelacionUtc));
        return Resultado.Exito();
    }

    private Error? ValidarVentaAbierta() =>
        Estado switch
        {
            EstadoVenta.Pagada => ErroresVenta.YaPagada,
            EstadoVenta.Cancelada => ErroresVenta.YaCancelada,
            _ => null
        };

    private DetalleVenta? BuscarDetalle(Guid detalleId) =>
        _detalles.FirstOrDefault(d => d.Id == detalleId);

    private void Tocar() => Revision = Guid.NewGuid();

    private Resultado ValidarTotalConDetalle(DetalleVenta detalleNuevo)
    {
        var montoTotal = _detalles.Sum(detalle => detalle.Subtotal.Monto) + detalleNuevo.Subtotal.Monto;
        return ValidarMontoTotal(montoTotal);
    }

    private Resultado ValidarTotalConCantidad(DetalleVenta detalle, Cantidad nuevaCantidad)
    {
        var resultadoNuevoSubtotal = detalle.CalcularSubtotal(nuevaCantidad);
        if (!resultadoNuevoSubtotal.EsExito)
        {
            return Resultado.Fallo(resultadoNuevoSubtotal.Error!);
        }

        var montoTotal = _detalles.Sum(item => item.Subtotal.Monto) - detalle.Subtotal.Monto +
            resultadoNuevoSubtotal.Valor!.Monto;
        return ValidarMontoTotal(montoTotal);
    }

    private static Resultado ValidarMontoTotal(decimal montoTotal)
    {
        var resultadoTotal = Dinero.Crear(montoTotal);
        return resultadoTotal.EsExito
            ? Resultado.Exito()
            : Resultado.Fallo(resultadoTotal.Error!);
    }

    private static bool EsFechaUtcValida(DateTime fechaUtc) =>
        fechaUtc != default && fechaUtc.Kind == DateTimeKind.Utc;

    private static Resultado<string> NormalizarMotivoCancelacion(string? motivoCancelacion)
    {
        if (string.IsNullOrWhiteSpace(motivoCancelacion))
        {
            return Resultado<string>.Fallo(ErroresVenta.MotivoCancelacionInvalido);
        }

        var motivoNormalizado = motivoCancelacion.Trim();
        if (motivoNormalizado.Length > LongitudMaximaMotivoCancelacion)
        {
            return Resultado<string>.Fallo(ErroresVenta.MotivoCancelacionInvalido);
        }

        return Resultado<string>.Exito(motivoNormalizado);
    }

    private Dinero? CalcularTotal()
    {
        if (_detalles.Count == 0)
        {
            return null;
        }

        Dinero? total = null;

        foreach (var detalle in _detalles)
        {
            var subtotal = detalle.Subtotal;
            total = total is null
                ? subtotal
                : total.Sumar(subtotal).Valor;
        }

        return total;
    }
}
