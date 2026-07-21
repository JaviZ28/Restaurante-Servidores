var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("restauranteventas-postgres-data");

var baseDatos = postgres.AddDatabase("restauranteventas");

builder.AddProject("api", "../src/RestauranteVentas.Api/RestauranteVentas.Api.csproj")
    .WithReference(baseDatos)
    .WaitFor(baseDatos)
    .WithHttpEndpoint(name: "http")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
