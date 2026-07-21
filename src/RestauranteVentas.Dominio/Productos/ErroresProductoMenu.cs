namespace RestauranteVentas.Dominio.Productos;

public static class ErroresProductoMenu
{
    public static readonly Abstracciones.Error NombreInvalido =
        new("ProductoMenu.NombreInvalido", "El nombre del producto debe ser válido.");

    public static readonly Abstracciones.Error PrecioInvalido =
        new("ProductoMenu.PrecioInvalido", "El precio del producto debe ser válido.");
}
