using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Application.ApplicationUsers.Services;
using ServerGame.Domain.Constants;
using ServerGame.Infrastructure.Database;
using ServerGame.Infrastructure.Database.Application;
using ServerGame.Infrastructure.Database.Application.Identity;
using ServerGame.Infrastructure.Database.Application.Identity.Entities;

namespace ServerGame.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.ConfigureDatabaseServices();

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
