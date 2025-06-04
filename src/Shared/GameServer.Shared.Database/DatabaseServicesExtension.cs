using Aspire.Npgsql.EntityFrameworkCore.PostgreSQL;
using GameServer.Shared.Database.Interceptors;
using GameServer.Shared.Database.Repository.Reader;
using GameServer.Shared.Database.Repository.Writer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameServer.Shared.Database;

public static class DatabaseServicesExtension
{
    /// <summary>
    /// Configura os serviços de banco de dados
    /// </summary>
    /// <param name="hostBuilder">Coleção de serviços</param>
    /// <param name="connectionStringName"></param>
    /// <param name="connectionName"></param>
    /// <param name="configure"></param>
    /// <param name="configureConnection"></param>
    /// <param name="optionsBuilder"></param>
    public static IHostApplicationBuilder ConfigureDatabaseServices<TEntity, TContext>(
        this IHostApplicationBuilder hostBuilder, 
        string connectionName,
        Action<DbContextOptionsBuilder>? optionsBuilder = null)
        where TEntity : class
        where TContext : DbContext
    {
        hostBuilder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        hostBuilder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        
        hostBuilder.Services.AddDbContext<TContext>((sp, options) =>
        {
            var connectionString = hostBuilder.Configuration.GetConnectionString(connectionName);
            
            // Use the connection string named "authdb" (will be injected by Aspire)
            if (string.IsNullOrEmpty(connectionString))
                // Fallback for local development
                options.UseInMemoryDatabase("{connectionStringName}-Memory");
            else
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                
                options.UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions
                        .EnableRetryOnFailure(3)).AddAsyncSeeding<TContext>(sp);
            }
            // Apply additional configuration if provided
        });
        
        hostBuilder.EnrichNpgsqlDbContext<TContext>();
         
        // Adiciona o repositório de escrita
        hostBuilder.Services.AddScoped<IWriterRepository<TEntity>>(opt =>
        {
            var dbContext = opt.GetRequiredService<TContext>();
            return new WriterRepository<TEntity>(dbContext, opt.GetRequiredService<ILogger<WriterRepository<TEntity>>>());
        });
        
        // Adiciona o repositório de leitura
        hostBuilder.Services.AddScoped<IReaderRepository<TEntity>>(opt =>
        {
            var dbContext = opt.GetRequiredService<TContext>();
            return new ReaderRepository<TEntity>(dbContext.Set<TEntity>());
        });
        
        return hostBuilder;
    }
}
