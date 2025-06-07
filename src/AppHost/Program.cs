using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", "aspire123!", secret: true);
var postgres = builder
    .AddPostgres("postgres", password: postgresPassword) // Fixed password for consistent authentication
    // Set the name of the default database to auto-create on container startup.
    .WithVolume("postgres-data", "/var/lib/postgresql/data") // <-- Volume aplicado CORRETAMENTE ao SERVIDOR
    .WithPgAdmin();

var serverdb = postgres.AddDatabase("serverdb", "serverdb");
builder
    .AddProject<Projects.API>("ApiService")
    .WithReference(serverdb)
    .WaitFor(serverdb);

builder.Build().Run();
