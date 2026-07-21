using RestauranteVentas.Aplicacion.Abstracciones;

namespace RestauranteVentas.Api.Infraestructura;

public sealed class GeneradorIdentidadGuid : IGeneradorIdentidad
{
    public Guid Nuevo() => Guid.NewGuid();
}
