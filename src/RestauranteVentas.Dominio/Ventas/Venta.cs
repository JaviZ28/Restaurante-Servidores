using RestauranteVentas.Dominio.Abstracciones;
using RestauranteVentas.Dominio.Compartido;
using RestauranteVentas.Dominio.Productos;
using RestauranteVentas.Dominio.Ventas.Eventos;

namespace RestauranteVentas.Dominio.Ventas;

public sealed class Venta : Entidad
{
    public Guid? ClienteId { get; private set; }
    public NumeroMesa? Mesa { get; private set; }
    public EstadoVenta Estado { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }
    public DateTime? FechaPagoUtc { get; private set; }
    public MetodoPago? MetodoPago { get; private set; }

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
    }

    public static Resultado<Venta> Crear(
        Guid id,
        Guid? clienteId,
        NumeroMesa? mesa,
        DateTime fechaCreacionUtc)
    {
        var venta = new Venta(id, clienteId, mesa, fechaCreacionUtc);
        venta.RegistrarEvento(new VentaCreadaEventoDominio(id, fechaCreacionUtc));
        return Resultado<Venta>.Exito(venta);
    }

    public Resultado AgregarProducto(ProductoMenu producto, Cantidad cantidad)
    {
        var errorEstado = ValidarVentaAbierta();
        if (errorEstado is not null)
        {
            return Resultado.Fallo(errorEstado);
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

        _detalles.Add(resultadoDetalle.Valor!);
        return Resultado.Exito();
    }

    public Resultado CambiarCantidad(Guid detalleId, Cantidad nuevaCantidad)
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

        return detalle.CambiarCantidad(nuevaCantidad);
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

        if (_detalles.Count == 0)
        {
            return Resultado.Fallo(ErroresVenta.SinDetalles);
        }

        Estado = EstadoVenta.Pagada;
        MetodoPago = metodoPago;
        FechaPagoUtc = fechaPagoUtc;
        RegistrarEvento(new VentaPagadaEventoDominio(Id, fechaPagoUtc));
        return Resultado.Exito();
    }

    public Resultado Cancelar(DateTime fechaCancelacionUtc)
    {
        if (Estado == EstadoVenta.Pagada)
        {
            return Resultado.Fallo(ErroresVenta.YaPagada);
        }

        if (Estado == EstadoVenta.Cancelada)
        {
            return Resultado.Fallo(ErroresVenta.YaCancelada);
        }

        Estado = EstadoVenta.Cancelada;
        RegistrarEvento(new VentaCanceladaEventoDominio(Id, fechaCancelacionUtc));
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
