using RestauranteVentas.Aplicacion.Abstracciones;

namespace RestauranteVentas.Infrastructure.Servicios;

/// <summary>
/// Adaptador del reloj del sistema. Application depende de la abstracción para
/// mantener los casos de uso deterministas en pruebas.
/// </summary>
public sealed class RelojSistema : IReloj
{
    public DateTime UtcNow => DateTime.UtcNow;
}
