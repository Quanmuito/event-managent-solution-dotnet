namespace AuthService.Data.Repositories;

using System.Text.RegularExpressions;
using AuthService.Data.Models;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

public class UserRepository(MongoDbContext mongoDbContext) : Repository<User>(mongoDbContext, "Users"), IUserRepository
{
    public async Task<List<User>> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync(cancellationToken);

        var escapedQuery = Regex.Escape(query);
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Regex(u => u.Email, new BsonRegularExpression(escapedQuery, "i")),
            Builders<User>.Filter.Regex(u => u.Phone, new BsonRegularExpression(escapedQuery, "i"))
        );

        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }
}
