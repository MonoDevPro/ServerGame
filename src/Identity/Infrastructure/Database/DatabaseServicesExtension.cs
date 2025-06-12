using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ServerGame.Application.Common.Interfaces.Database;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Application.Common.Interfaces.Dispatchers;
using ServerGame.Domain.Entities.Accounts;
using ServerGame.Domain.Events;
using ServerGame.Infrastructure.Database.Application;
using ServerGame.Infrastructure.Database.Application.Identity.Entities;
using ServerGame.Infrastructure.Database.Common.Dispatchers;
using ServerGame.Infrastructure.Database.Common.Interceptors.Interfaces;
using ServerGame.Infrastructure.Database.Common.Interceptors.Services;
using ServerGame.Infrastructure.Database.Common.Repositories;
using ServerGame.Infrastructure.Database.Common.Repositories.Reader;
using ServerGame.Infrastructure.Database.Common.Repositories.Writer;
using ServerGame.Infrastructure.Database.Domain;
using ServerGame.Infrastructure.Database.Domain.Interceptors;

namespace ServerGame.Infrastructure.Database;

public static class DatabaseServicesExtension
{
    
    public static IHostApplicationBuilder ConfigureDatabaseServices(
        this IHostApplicationBuilder hostBuilder)
    {
        var connectionName = "serverdb";
        
        hostBuilder.Services.AddScoped<IDatabaseSeeding, DbContextInitializer>();
        
        hostBuilder.Services.AddScoped<INotificationDispatcher<INotification>, NotificationDispatcher>();
        hostBuilder.Services.AddScoped<IEventDispatcher<IDomainEvent>, EventDispatcher>();
        hostBuilder.Services.AddScoped<ISaveChangesInterceptor, DatabaseInterceptor>();
        hostBuilder.Services.TryAddScoped<IPreSaveInterceptor, AuditableInterceptor>();
        hostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IPreSaveInterceptor, EntityEventDispatcherInterceptor>());
        hostBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IPostSaveInterceptor, EntityEventDispatcherInterceptor>());

        // Identity (ApplicationDbContext) ─ migrações em Assembly: IdentityMigrations
        hostBuilder.Services.AddDbContext<ApplicationDbContext>((sp, opt) =>
        {
            var cs = sp.GetRequiredService<IConfiguration>()
                       .GetConnectionString(connectionName);
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(cs, npg =>
            {
                npg.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npg.EnableRetryOnFailure(3);
            });
        });

        // Domain (DomainDbContext) ─ migrações em Assembly: DomainMigrations
        hostBuilder.Services.AddDbContext<DomainDbContext>((sp, opt) =>
        {
            var cs = sp.GetRequiredService<IConfiguration>()
                .GetConnectionString(connectionName);
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(cs, npg =>
            {
                npg.MigrationsAssembly(typeof(DomainDbContext).Assembly.FullName);
                npg.EnableRetryOnFailure(3);
            });
        });
        
        hostBuilder.EnrichNpgsqlDbContext<DomainDbContext>();
        hostBuilder.EnrichNpgsqlDbContext<ApplicationDbContext>();

        // Registrar repositórios de cada DbContext
        RegisterRepositoriesFor<DomainDbContext>(hostBuilder,typeof(Account) /* entidades de domínio */);
        RegisterRepositoriesFor<ApplicationDbContext>(hostBuilder, typeof(ApplicationUser) /* entidades de identity */);
        
        return hostBuilder;
    }
    
    private static void RegisterRepositoriesFor<TContext>(
        IHostApplicationBuilder hostBuilder,
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
            // compose
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
