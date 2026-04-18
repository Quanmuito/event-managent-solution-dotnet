namespace AuthService.Data.Repositories;

using AuthService.Data.Models;
using DatabaseService.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<List<User>> SearchAsync(string? query, CancellationToken cancellationToken);
}
