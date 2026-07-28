using System.Reflection;
using RestauranteVentas.Api;
using RestauranteVentas.Aplicacion.Abstracciones;
using RestauranteVentas.Dominio.Ventas;
using RestauranteVentas.Infrastructure;

namespace RestauranteVentas.Arquitectura.Tests;

/// <summary>
/// Estas pruebas protegen la dirección de dependencias acordada. No sustituyen
/// el diseño, pero evitan regresiones silenciosas que romperían Clean Architecture.
/// </summary>
public sealed class DependenciasArquitecturaTests
{
    [Fact]
    public void Dominio_no_depende_de_las_capas_externas()
    {
        var referencias = ReferenciasDe(typeof(Venta).Assembly);

        Assert.DoesNotContain("RestauranteVentas.Aplicacion", referencias);
        Assert.DoesNotContain("RestauranteVentas.Infrastructure", referencias);
        Assert.DoesNotContain("RestauranteVentas.Api", referencias);
    }

    [Fact]
    public void Aplicacion_depende_solo_del_dominio_dentro_de_la_solucion()
    {
        var referencias = ReferenciasDe(typeof(IUnidadDeTrabajo).Assembly);

        Assert.Contains("RestauranteVentas.Dominio", referencias);
        Assert.DoesNotContain("RestauranteVentas.Infrastructure", referencias);
        Assert.DoesNotContain("RestauranteVentas.Api", referencias);
    }

    [Fact]
    public void Infraestructura_implementa_puertos_sin_depender_de_api()
    {
        var referencias = ReferenciasDe(typeof(DependenciaInyeccion).Assembly);

        Assert.Contains("RestauranteVentas.Aplicacion", referencias);
        Assert.Contains("RestauranteVentas.Dominio", referencias);
        Assert.DoesNotContain("RestauranteVentas.Api", referencias);
    }

    [Fact]
    public void Api_es_el_borde_que_compone_aplicacion_e_infraestructura()
    {
        var referencias = ReferenciasDe(typeof(Program).Assembly);

        Assert.Contains("RestauranteVentas.Aplicacion", referencias);
        Assert.Contains("RestauranteVentas.Infrastructure", referencias);
    }

    private static IReadOnlyCollection<string> ReferenciasDe(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(referencia => referencia.Name!)
            .ToArray();
}
