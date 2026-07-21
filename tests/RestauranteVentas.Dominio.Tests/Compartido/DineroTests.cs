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
    public void Sumar_montos_usd_calcula_total()
    {
        var usd = Dinero.Crear(10m).Valor!;
        var otroUsd = Dinero.Crear(5m).Valor!;

        var resultado = usd.Sumar(otroUsd);

        Assert.True(resultado.EsExito);
        Assert.Equal(15m, resultado.Valor!.Monto);
        Assert.Equal(Dinero.MonedaUsd, resultado.Valor.Moneda);
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
