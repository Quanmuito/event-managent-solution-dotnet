namespace UserService.Data.Repositories;

using DatabaseService.Repositories;
using UserService.Data.Models;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetActiveByIdAsync(string id, CancellationToken cancellationToken);
    Task<List<User>> SearchAsync(string? query, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> SoftDeleteAsync(string id, CancellationToken cancellationToken);
}
