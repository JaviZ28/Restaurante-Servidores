using Xunit;

// Cada prueba inicia su propio AppHost, PostgreSQL y API. Ejecutarlas en paralelo
// puede provocar que algunos recursos no alcancen a iniciar correctamente.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
