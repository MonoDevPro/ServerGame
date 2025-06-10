using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Events;
using ServerGame.Application.Users.Services;
using ServerGame.Domain.Constants;
using ServerGame.Domain.Entities;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Infrastructure.Data;
using ServerGame.Infrastructure.Data.Context;
using ServerGame.Infrastructure.Data.Events;
using ServerGame.Infrastructure.Identity;
using ServerGame.Infrastructure.Identity.Entities;

namespace ServerGame.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        
        builder.Services.AddScoped<IDatabaseSeeding, DbContextInitializer>();

        builder.ConfigureDatabaseServicesWithAction<ApplicationDbContext>(
            connectionName: "serverdb",
            entityConfigurator: a => a.AddEntity<Account>(),
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
        
        builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ClaimsPrincipalFactory>();
        
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));
        });
    }
}
