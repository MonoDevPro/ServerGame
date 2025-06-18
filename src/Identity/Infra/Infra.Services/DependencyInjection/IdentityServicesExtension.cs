using Infra.Identity.Factories;
using Infra.Identity.Persistence.DbContexts;
using Infra.Identity.Persistence.Entities;
using Infra.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Application.Common.Interfaces.Identity;
using ServerGame.Domain.Constants;

namespace Infra.Services.DependencyInjection;

public static class IdentityServicesExtension
{
    public static IHostApplicationBuilder ConfigureIdentityServices(
        this IHostApplicationBuilder hostBuilder,
        Action<DbContextOptionsBuilder, IServiceProvider> contextOptionsBuilder)
    {
        hostBuilder.Services.AddDbContext<ApplicationDbContext>((sp, opt) =>
        {
            contextOptionsBuilder.Invoke(opt, sp);
        });
        hostBuilder.EnrichNpgsqlDbContext<ApplicationDbContext>();
        
        hostBuilder.Services
            .AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);
        
        hostBuilder.Services.AddAuthorizationBuilder();

        hostBuilder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();
        
        hostBuilder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ClaimsPrincipalFactory>();
        hostBuilder.Services.AddScoped<IIdentityService, IdentityService>();
        
        hostBuilder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        return hostBuilder;
    }
}
