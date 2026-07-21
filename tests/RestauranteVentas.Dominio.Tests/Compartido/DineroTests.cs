using RestauranteVentas.Dominio.Compartido;

namespace RestauranteVentas.Dominio.Tests.Compartido;

public class DineroTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_rechaza_monto_invalido(decimal monto)
    {
        var resultado = Dinero.Crear(monto);

        Assert.False(resultado.EsExito);
        Assert.Equal(Dinero.CodigoMontoInvalido, resultado.Error!.Codigo);
    }

    [Fact]
    public void Crear_rechaza_moneda_diferente_de_usd()
    {
        var resultado = Dinero.Crear(10m, "EUR");

        Assert.False(resultado.EsExito);
        Assert.Equal(Dinero.CodigoMonedaInvalida, resultado.Error!.Codigo);
    }

    [Fact]
    public void Sumar_en_monedas_diferentes_falla()
    {
        var usd = Dinero.Crear(10m).Valor!;
        var otroUsd = Dinero.Crear(5m).Valor!;

        var resultadoValido = usd.Sumar(otroUsd);
        Assert.True(resultadoValido.EsExito);
        Assert.Equal(15m, resultadoValido.Valor!.Monto);

        var dineroReflexion = typeof(Dinero).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            [typeof(decimal), typeof(string)],
            null)!;

        var dineroCop = (Dinero)dineroReflexion.Invoke([10m, "EUR"])!;
        var resultadoInvalido = usd.Sumar(dineroCop);

        Assert.False(resultadoInvalido.EsExito);
        Assert.Equal(Dinero.CodigoMonedaDistinta, resultadoInvalido.Error!.Codigo);
    }

    [Fact]
    public void Multiplicar_precio_por_cantidad_calcula_subtotal()
    {
        var precio = Dinero.Crear(12.5m).Valor!;
        var cantidad = Cantidad.Crear(3).Valor!;

        var resultado = precio.Multiplicar(cantidad);

        Assert.True(resultado.EsExito);
        Assert.Equal(37.5m, resultado.Valor!.Monto);
        Assert.Equal(Dinero.MonedaUsd, resultado.Valor.Moneda);
    }
}
