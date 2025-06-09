using ServerGame.Api;
using ServerGame.Api.Infrastructure;
using ServerGame.Infrastructure;
using ServerGame.Infrastructure.Data;
using ServerGame.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    // Só em Development: inicializa bancos (migrate/ensure)
    await app.InitialiseDatabaseAsync();

// A partir daqui, nunca execute MigrateAsync() em Production ou no NSwag
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();

namespace ServerGame.Api
{
    public partial class Program
    {
        // This class is used to allow the Program class to be partial, which is required for the WebApplicationBuilder
        // to be created in the Program.cs file.
        // It also allows the Program class to be used in tests.
    }
}
