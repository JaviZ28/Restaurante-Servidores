using RestauranteVentas.Dominio.Compartido;

namespace RestauranteVentas.Dominio.Tests.Compartido;

public class NombreProductoTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_rechaza_vacio(string nombre)
    {
        var resultado = NombreProducto.Crear(nombre);

        Assert.False(resultado.EsExito);
        Assert.Equal(NombreProducto.CodigoVacio, resultado.Error!.Codigo);
    }

    [Fact]
    public void Crear_rechaza_nombre_demasiado_largo()
    {
        var nombre = new string('a', NombreProducto.LongitudMaxima + 1);
        var resultado = NombreProducto.Crear(nombre);

        Assert.False(resultado.EsExito);
        Assert.Equal(NombreProducto.CodigoLongitudInvalida, resultado.Error!.Codigo);
    }
}
