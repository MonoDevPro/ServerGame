using System.Reflection;
using GameServer.Application.Common.Interfaces.Identity;
using GameServer.Domain.Constants;
using GameServer.Infrastructure.Identity.Factories;
using GameServer.Infrastructure.Identity.Persistence.DbContexts;
using GameServer.Infrastructure.Identity.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameServer.Infrastructure.Identity;

public static class DependencyInjection
{
    public static IHostApplicationBuilder ConfigureIdentityServices(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IIdentityService, IdentityService>();
        
        builder.Services
            .AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);
        
        builder.Services.AddAuthorizationBuilder();
        
        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
        
        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddClaimsPrincipalFactory<ClaimsPrincipalFactory>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();
        
        var connectionString = builder.Configuration.GetConnectionString("serverdb");
        Guard.Against.Null(connectionString, message: "Connection string 'serverdb' not found.");
        
        builder.Services.AddDbContext<ApplicationDbContext>((sp, opt) =>
        {
            opt
                .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                .UseNpgsql(connectionString, npg =>
                {
                    npg.MigrationsAssembly(Assembly.GetExecutingAssembly());
                    npg.EnableRetryOnFailure(3);
                });
        });
        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();
        
        return builder;
    }
}
