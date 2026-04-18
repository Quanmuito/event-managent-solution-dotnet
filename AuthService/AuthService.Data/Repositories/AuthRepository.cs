namespace AuthService.Data.Repositories;

using AuthService.Data.Models;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Driver;

public class AuthRepository(MongoDbContext mongoDbContext) : Repository<Auth>(mongoDbContext, "Auths"), IAuthRepository
{
    public async Task<List<Auth>> GetByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        var filter = Builders<Auth>.Filter.Eq(a => a.UserId, userId);
        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }
}
