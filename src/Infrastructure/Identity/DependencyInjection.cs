using System.Reflection;
using GameServer.Application.Common.Interfaces.Identity;
using GameServer.Domain.Constants;
using GameServer.Infrastructure.Identity.Claims;
using GameServer.Infrastructure.Services;
using Identity.Persistence;
using Identity.Persistence.DbContexts;
using Identity.Persistence.Entities;
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
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();
        
        builder.Services
            .AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);
        
        builder.Services.AddAuthorizationBuilder();
        
        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        builder.ConfigureIdentityPersistence();
        
        return builder;
    }
}
