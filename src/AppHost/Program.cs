using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var databaseName = "ServerGameDb";

var postgres = builder
    .AddPostgres("postgres")
    // Set the name of the default database to auto-create on container startup.
    .WithEnvironment("POSTGRES_DB", databaseName)
    .WithDataVolume("postgres-data"); // Volume for PostgreSQL data

var database = postgres.AddDatabase(databaseName);

builder
    .AddProject<API>("ApiService")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
