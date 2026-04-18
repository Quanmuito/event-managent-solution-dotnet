namespace AuthService.Data.Repositories;

using AuthService.Data.Models;
using DatabaseService.Repositories;

public interface IAuthRepository : IRepository<Auth>
{
    Task<List<Auth>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
}
