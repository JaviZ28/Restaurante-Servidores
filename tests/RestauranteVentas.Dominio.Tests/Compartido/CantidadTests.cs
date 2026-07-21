using RestauranteVentas.Dominio.Compartido;

namespace RestauranteVentas.Dominio.Tests.Compartido;

public class CantidadTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Crear_rechaza_cero_y_negativos(int valor)
    {
        var resultado = Cantidad.Crear(valor);

        Assert.False(resultado.EsExito);
        Assert.Equal(Cantidad.CodigoInvalida, resultado.Error!.Codigo);
    }

    [Fact]
    public void Crear_acepta_valor_positivo()
    {
        var resultado = Cantidad.Crear(2);

        Assert.True(resultado.EsExito);
        Assert.Equal(2, resultado.Valor!.Valor);
    }
}
