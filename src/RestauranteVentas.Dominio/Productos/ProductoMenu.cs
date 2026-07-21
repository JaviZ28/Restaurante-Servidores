using RestauranteVentas.Dominio.Abstracciones;
using RestauranteVentas.Dominio.Compartido;

namespace RestauranteVentas.Dominio.Productos;

public sealed class ProductoMenu : Entidad
{
    public NombreProducto Nombre { get; private set; }
    public Dinero PrecioActual { get; private set; }
    public bool EstaActivo { get; private set; }

    private ProductoMenu(Guid id, NombreProducto nombre, Dinero precio)
        : base(id)
    {
        Nombre = nombre;
        PrecioActual = precio;
        EstaActivo = true;
    }

    public static Resultado<ProductoMenu> Crear(Guid id, NombreProducto? nombre, Dinero? precio)
    {
        if (id == Guid.Empty)
        {
            return Resultado<ProductoMenu>.Fallo(ErroresProductoMenu.IdInvalido);
        }

        if (nombre is null)
        {
            return Resultado<ProductoMenu>.Fallo(ErroresProductoMenu.NombreInvalido);
        }

        if (precio is null)
        {
            return Resultado<ProductoMenu>.Fallo(ErroresProductoMenu.PrecioInvalido);
        }

        return Resultado<ProductoMenu>.Exito(new ProductoMenu(id, nombre, precio));
    }

    public Resultado ActualizarPrecio(Dinero? nuevoPrecio)
    {
        if (nuevoPrecio is null)
        {
            return Resultado.Fallo(ErroresProductoMenu.PrecioInvalido);
        }

        PrecioActual = nuevoPrecio;
        return Resultado.Exito();
    }

    public Resultado CambiarNombre(NombreProducto? nuevoNombre)
    {
        if (nuevoNombre is null)
        {
            return Resultado.Fallo(ErroresProductoMenu.NombreInvalido);
        }

        Nombre = nuevoNombre;
        return Resultado.Exito();
    }

    public Resultado Activar()
    {
        EstaActivo = true;
        return Resultado.Exito();
    }

    public Resultado Desactivar()
    {
        EstaActivo = false;
        return Resultado.Exito();
    }
}
