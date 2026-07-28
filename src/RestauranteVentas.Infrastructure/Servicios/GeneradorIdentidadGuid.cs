using RestauranteVentas.Aplicacion.Abstracciones;

namespace RestauranteVentas.Infrastructure.Servicios;

/// <summary>
/// Adaptador de identidad técnica. El dominio nunca genera infraestructura ni
/// depende de Guid.NewGuid directamente para crear agregados desde casos de uso.
/// </summary>
public sealed class GeneradorIdentidadGuid : IGeneradorIdentidad
{
    public Guid Nuevo() => Guid.NewGuid();
}
