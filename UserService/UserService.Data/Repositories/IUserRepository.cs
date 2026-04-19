namespace UserService.Data.Repositories;

using DatabaseService.Repositories;
using UserService.Data.Models;

public interface IUserRepository : IRepository<User>
{
    Task<List<User>> SearchAsync(string? query, CancellationToken cancellationToken);
}
