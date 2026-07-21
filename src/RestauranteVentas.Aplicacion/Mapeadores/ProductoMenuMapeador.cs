using RestauranteVentas.Aplicacion.Dtos;
using RestauranteVentas.Dominio.Productos;

namespace RestauranteVentas.Aplicacion.Mapeadores;

public static class ProductoMenuMapeador
{
    public static ProductoMenuDto ADto(this ProductoMenu producto) =>
        new(
            producto.Id,
            producto.Nombre.Valor,
            producto.PrecioActual.Monto,
            producto.PrecioActual.Moneda,
            producto.EstaActivo);
}
