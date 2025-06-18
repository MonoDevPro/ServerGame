using System.Reflection;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerGame.Application.Common.Interfaces.Persistence.Repository;
using ServerGame.Infrastructure.Persistence.Repositories;
using ServerGame.Infrastructure.Persistence.Repositories.Reader;
using ServerGame.Infrastructure.Persistence.Repositories.Writer;

namespace Infra.Services.DependencyInjection;

public static class CommonServicesExtensions
{
    public static DbContextOptionsBuilder AddConnectionString(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Assembly? assembly = null)
    {
        return optionsBuilder.UseNpgsql(connectionString, npg =>
        {
            npg.MigrationsAssembly(assembly?.FullName);
            npg.EnableRetryOnFailure(3);
        });
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
