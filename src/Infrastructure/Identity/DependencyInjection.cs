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
            
            options
                .AddPolicy(Policies.GameSession, policy 
                    => policy.RequireAssertion(context =>
                    {
                        var accountClaim = context.User.FindFirst(Domain.Constants.Claims.AccountId);
                        var sessionTimeClaim = context.User.FindFirst(Domain.Constants.Claims.SessionStartTime);
                        
                        if (accountClaim == null || sessionTimeClaim == null)
                            return false;

                        // Verificar se a sessão não é muito antiga (opcional)
                        if (!long.TryParse(sessionTimeClaim.Value, out var timestamp))
                            return true;

                        var sessionStart = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                        var maxAge = TimeSpan.FromHours(8);
                        return DateTimeOffset.UtcNow - sessionStart < maxAge;
                    }));
        });

        builder.ConfigureIdentityPersistence();

        return builder;
    }
}
