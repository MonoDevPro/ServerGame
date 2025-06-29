using System.Reflection;
using Ardalis.GuardClauses;
using Game.Persistence.DbContexts;
using Game.Persistence.Hosted;
using Game.Persistence.Interceptors;
using Game.Persistence.Repositories;
using Game.Persistence.Repositories.Reader;
using Game.Persistence.Repositories.Writer;
using Game.Persistence.UnitOfWork;
using GameServer.Application.Common.Interfaces.Persistence;
using GameServer.Application.Common.Interfaces.Persistence.Repository;
using GameServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Game.Persistence;

public static class DependencyInjection
{
    public static IHostApplicationBuilder ConfigurePersistence(
        this IHostApplicationBuilder builder)
    {
        // Persistence
        var connectionString = builder.Configuration.GetConnectionString("serverdb");
        Guard.Against.Null(connectionString, message: "Connection string 'serverdb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, NotificationInterceptor>();

        builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        builder.Services.AddHostedService<ApplyMigrationsHosted>();

        // Game
        builder.Services.AddDbContext<GameDbContext>((sp, opt) =>
        {
            opt
                .UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                .UseNpgsql(connectionString, npg =>
                {
                    npg.MigrationsAssembly(Assembly.GetExecutingAssembly());
                    npg.EnableRetryOnFailure(3);
                });
        });
        builder.EnrichNpgsqlDbContext<GameDbContext>();
        builder.RegisterRepositoriesFor<GameDbContext>(
            typeof(Account), /* entidades de domínio */
            typeof(Character));

        return builder;
    }

    public static void RegisterRepositoriesFor<TContext>(
        this IHostApplicationBuilder hostBuilder,
        params Type[] entityTypes)
        where TContext : DbContext
    {
        foreach (var et in entityTypes)
        {
            // writer
            hostBuilder.Services.AddScoped(
                typeof(IWriterRepository<>).MakeGenericType(et),
                sp =>
                {
                    var ctx = sp.GetRequiredService<TContext>();
                    var repoType = typeof(WriterRepository<>).MakeGenericType(et);
                    var instance = Activator.CreateInstance(repoType, ctx, sp.GetRequiredService<ILoggerFactory>().CreateLogger(repoType));
                    return Guard.Against.Null(instance);
                });
            // reader
            hostBuilder.Services.AddScoped(
                typeof(IReaderRepository<>).MakeGenericType(et),
                sp =>
                {
                    var ctx = sp.GetRequiredService<TContext>();
                    var repoType = typeof(ReaderRepository<>).MakeGenericType(et);
                    var instance = Activator.CreateInstance(repoType, ctx);
                    return Guard.Against.Null(instance);
                });

            hostBuilder.Services.AddScoped(
                typeof(IRepositoryCompose<>).MakeGenericType(et),
                sp =>
                {
                    var writer = sp.GetRequiredService(
                        typeof(IWriterRepository<>).MakeGenericType(et));
                    var reader = sp.GetRequiredService(
                        typeof(IReaderRepository<>).MakeGenericType(et));
                    var repoType = typeof(RepositoryCompose<>).MakeGenericType(et);

                    var instance = Activator.CreateInstance(repoType, writer, reader);
                    return Guard.Against.Null(instance);
                });
        }
    }
}
