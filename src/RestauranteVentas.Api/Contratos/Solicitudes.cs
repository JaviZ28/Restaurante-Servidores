namespace RestauranteVentas.Api.Contratos;

public sealed record CrearProductoSolicitud(string Nombre, decimal Precio);

public sealed record CrearVentaSolicitud(Guid? ClienteId, int? NumeroMesa);

public sealed record AgregarDetalleVentaSolicitud(Guid ProductoMenuId, int Cantidad);

public sealed record CambiarCantidadDetalleVentaSolicitud(int NuevaCantidad);

public sealed record PagarVentaSolicitud(string MetodoPago);
