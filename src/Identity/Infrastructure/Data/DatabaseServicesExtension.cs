using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using ServerGame.Application.Common.Interfaces.Database.Repository;
using ServerGame.Infrastructure.Data.Interceptors;
using ServerGame.Infrastructure.Data.Repositories;
using ServerGame.Infrastructure.Data.Repositories.Reader;
using ServerGame.Infrastructure.Data.Repositories.Writer;

namespace ServerGame.Infrastructure.Data;

public static class DatabaseServicesExtension
{
    /// <summary>
    /// Configura os serviços de banco de dados com suporte para uma entidade
    /// </summary>
    /// <param name="hostBuilder">Coleção de serviços</param>
    /// <param name="connectionName">Nome da string de conexão</param>
    /// <param name="postgreDbContextSettings">Configurações do provedor PostgreSQL</param>
    /// <param name="optionsBuilder">Configurações adicionais do contexto</param>
    public static IHostApplicationBuilder ConfigureDatabaseServices<TEntity, TContext>(
        this IHostApplicationBuilder hostBuilder, 
        string connectionName,
        Action<NpgsqlDbContextOptionsBuilder>? postgreDbContextSettings = null,
        Action<DbContextOptionsBuilder>? optionsBuilder = null)
        where TEntity : class
        where TContext : DbContext
    {
        // Configura o DbContext e os interceptors
        ConfigureDbContext<TContext>(hostBuilder, connectionName, postgreDbContextSettings, optionsBuilder);
        
        // Registra os repositórios para a entidade especificada
        RegisterEntityRepositories<TEntity, TContext>(hostBuilder);
        
        return hostBuilder;
    }

    /// <summary>
    /// Configura os serviços de banco de dados com suporte para múltiplas entidades usando expressões lambda
    /// </summary>
    /// <param name="hostBuilder">Coleção de serviços</param>
    /// <param name="connectionName">Nome da string de conexão</param>
    /// <param name="entityConfigurator">Configurador de entidades</param>
    /// <param name="postgreDbContextSettings">Configurações do provedor PostgreSQL</param>
    /// <param name="optionsBuilder">Configurações adicionais do contexto</param>
    public static IHostApplicationBuilder ConfigureDatabaseServicesWithAction<TContext>(
        this IHostApplicationBuilder hostBuilder,
        string connectionName,
        Action<IEntityTypeRegistrar<TContext>> entityConfigurator,
        Action<NpgsqlDbContextOptionsBuilder>? postgreDbContextSettings = null,
        Action<DbContextOptionsBuilder>? optionsBuilder = null)
        where TContext : DbContext
    {
        // Configura o DbContext e os interceptors
        ConfigureDbContext<TContext>(hostBuilder, connectionName, postgreDbContextSettings, optionsBuilder);
        
        // Usa o registrador para configurar as entidades
        var registrar = new EntityTypeRegistrar<TContext>(hostBuilder);
        entityConfigurator(registrar);
        
        return hostBuilder;
    }

    /// <summary>
    /// Interface para registro de entidades
    /// </summary>
    public interface IEntityTypeRegistrar<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Registra um tipo de entidade para uso com repositórios
        /// </summary>
        IEntityTypeRegistrar<TContext> AddEntity<TEntity>() where TEntity : class;
    }

    /// <summary>
    /// Implementação do registrador de entidades
    /// </summary>
    private class EntityTypeRegistrar<TContext> : IEntityTypeRegistrar<TContext> where TContext : DbContext
    {
        private readonly IHostApplicationBuilder _hostBuilder;

        public EntityTypeRegistrar(IHostApplicationBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        public IEntityTypeRegistrar<TContext> AddEntity<TEntity>() where TEntity : class
        {
            RegisterEntityRepositories<TEntity, TContext>(_hostBuilder);
            return this;
        }
    }

    /// <summary>
    /// Registra os repositórios para um tipo de entidade específico
    /// </summary>
    private static void RegisterEntityRepositories<TEntity, TContext>(IHostApplicationBuilder hostBuilder)
        where TEntity : class
        where TContext : DbContext
    {
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
            return new ReaderRepository<TEntity>(dbContext.Set<TEntity>().AsQueryable());
        });
        
        // Registra o compose de repositórios
        hostBuilder.Services.AddScoped<IRepositoryCompose<TEntity>>(opt =>
        {
            var writerRepo = opt.GetRequiredService<IWriterRepository<TEntity>>();
            var readerRepo = opt.GetRequiredService<IReaderRepository<TEntity>>();
            return new RepositoryCompose<TEntity>(writerRepo, readerRepo);
        });
    }

    /// <summary>
    /// Configura o DbContext e os interceptors
    /// </summary>
    private static void ConfigureDbContext<TContext>(
        IHostApplicationBuilder hostBuilder, 
        string connectionName,
        Action<NpgsqlDbContextOptionsBuilder>? postgreDbContextSettings,
        Action<DbContextOptionsBuilder>? optionsBuilder)
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
                options.UseInMemoryDatabase($"{connectionName}-Memory");
            else
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                
                options.UseNpgsql(connectionString, postgreDbContextSettings)
                    .AddAsyncSeeding(sp);
            }
            // Apply additional configuration if provided
            optionsBuilder?.Invoke(options);
        });
        
        hostBuilder.EnrichNpgsqlDbContext<TContext>();
    }
}

