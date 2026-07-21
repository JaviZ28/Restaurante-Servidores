using RestauranteVentas.Aplicacion.Abstracciones;

namespace RestauranteVentas.Api.Infraestructura;

public sealed class RelojSistema : IReloj
{
    public DateTime UtcNow => DateTime.UtcNow;
}
