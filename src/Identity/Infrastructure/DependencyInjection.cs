using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Infrastructure.Data;
using ServerGame.Infrastructure.Data.Context;
using ServerGame.Infrastructure.Identity;

namespace ServerGame.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDatabaseSeeding, DbContextInitializer>();

        builder.ConfigureDatabaseServicesWithAction<ApplicationDbContext>(
            connectionName: "authdb",
            entityConfigurator: a => a.AddEntity<Account>(),
            optionsBuilder: null,
            postgreDbContextSettings: opt =>
            {
                opt.EnableRetryOnFailure(3);
            });
        
        builder.ConfigureDatabaseServices<ApplicationUser, ApplicationDbContext>(
            connectionName: "authdb",
            optionsBuilder: null,
            postgreDbContextSettings: opt =>
            {
                opt.EnableRetryOnFailure(3);
            });

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }
}
