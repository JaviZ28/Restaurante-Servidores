using RestauranteVentas.Dominio.Compartido;

namespace RestauranteVentas.Dominio.Tests.Compartido;

public class NumeroMesaTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_rechaza_cero_y_negativos(int valor)
    {
        var resultado = NumeroMesa.Crear(valor);

        Assert.False(resultado.EsExito);
        Assert.Equal(NumeroMesa.CodigoInvalido, resultado.Error!.Codigo);
    }
}
