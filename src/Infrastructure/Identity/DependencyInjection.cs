using GameServer.Application.Common.Interfaces;
using GameServer.Application.Common.Interfaces.Identity;
using GameServer.Domain.Constants;
using GameServer.Infrastructure.Identity.Claims;
using Identity.Persistence;
using Identity.Persistence.DbContexts;
using Identity.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameServer.Infrastructure.Identity;

public static class DependencyInjection
{
    public static IHostApplicationBuilder ConfigureIdentityServices(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IGameSessionService, GameSessionService>();
        builder.Services.AddScoped<IIdentityService, IdentityService>();

        // Adicionar memória transitória como cache
        builder.Services.AddMemoryCache();

        // Adicionar métricas
        builder.Services.AddMetrics();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<ClaimsPrincipalFactory>()
            .AddApiEndpoints();

        builder.Services
            .AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services.AddAuthorization(options =>
        {
            options
                .AddPolicy(Policies.CanPurge, policy
                    => policy.RequireRole(Roles.Administrator));

        });

        builder.ConfigureIdentityPersistence();

        return builder;
    }
}
