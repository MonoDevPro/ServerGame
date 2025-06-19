namespace GameServer.Application.Common.Interfaces.Persistence.Repository;

public interface IRepositoryCompose<TEntity>
    where TEntity : class
{
    public IWriterRepository<TEntity> WriterRepository { get; }
    public IReaderRepository<TEntity> ReaderRepository { get; }
}
