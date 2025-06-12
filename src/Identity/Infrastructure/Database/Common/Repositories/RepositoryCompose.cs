using ServerGame.Application.Common.Interfaces.Database.Repository;

namespace ServerGame.Infrastructure.Database.Common.Repositories;

internal class RepositoryCompose<TEntity> : IRepositoryCompose<TEntity>
    where TEntity : class
{
    public IWriterRepository<TEntity> WriterRepository { get; }
    public IReaderRepository<TEntity> ReaderRepository { get; }
    
    public RepositoryCompose(IWriterRepository<TEntity> writerRepository, IReaderRepository<TEntity> readerRepository)
    {
        WriterRepository = writerRepository;
        ReaderRepository = readerRepository;
    }
}
