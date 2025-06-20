using System.Reflection;
using Ardalis.GuardClauses;
using GameServer.Domain.Constants;
using Identity.Persistence.DbContexts;
using Identity.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.Persistence;

public static class DependencyInjection
{
    public static IHostApplicationBuilder ConfigureIdentityPersistence(
        this IHostApplicationBuilder builder)
    {
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
