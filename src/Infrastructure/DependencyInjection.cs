using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerGame.Application.Common.Interfaces;
using ServerGame.Domain.Constants;
using ServerGame.Infrastructure.Authentication;
using ServerGame.Infrastructure.Data;
using ServerGame.Infrastructure.Data.Interceptors;
using ServerGame.Infrastructure.Identity;

namespace ServerGame.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("ServerGameDb");
            Guard.Against.Null(connectionString, message: "Connection string 'ServerGameDb' not found.");

            if (string.IsNullOrEmpty(connectionString))
                options.UseInMemoryDatabase("servergame-local");
            else
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.EnableRetryOnFailure(3)).AddAsyncSeeding(sp);
            }
            
            options.UseOpenIddict();
        });

        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.AddOpenIddictBuilder();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddBearerToken(IdentityConstants.BearerScheme)
            .AddCookie(options =>
        {
            options.AccessDeniedPath = "/connect/signin";
            options.LoginPath = "/connect/signin";
            options.LogoutPath = "/connect/signout";
        });

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
