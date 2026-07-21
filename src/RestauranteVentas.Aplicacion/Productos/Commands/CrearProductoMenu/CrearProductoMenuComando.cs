using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Aplicacion.Dtos;

namespace RestauranteVentas.Aplicacion.Productos.Commands.CrearProductoMenu;

public sealed record CrearProductoMenuComando(
    string Nombre,
    decimal Precio)
    : IComando<ResultadoAplicacion<ProductoMenuDto>>;
