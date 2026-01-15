namespace DatabaseService.Repositories;

using MongoDB.Driver;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken);
    Task<T> UpdateAsync(string id, UpdateDefinition<T> updateDefinition, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}
