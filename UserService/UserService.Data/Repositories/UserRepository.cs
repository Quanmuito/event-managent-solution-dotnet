namespace UserService.Data.Repositories;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using UserService.Data.Models;

public class UserRepository(MongoDbContext mongoDbContext) : Repository<User>(mongoDbContext, "Users"), IUserRepository
{
    public async Task<User> GetActiveByIdAsync(string id, CancellationToken cancellationToken)
    {
        var objectId = GetValidObjectId(id);
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", objectId),
            Builders<User>.Filter.Eq(u => u.IsDeleted, false)
        );

        var result = await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return result ?? throw new KeyNotFoundException($"Users with ID '{id}' was not found.");
    }

    public async Task<List<User>> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        var activeFilter = Builders<User>.Filter.Eq(u => u.IsDeleted, false);

        if (string.IsNullOrWhiteSpace(query))
            return await Collection.Find(activeFilter).ToListAsync(cancellationToken);

        var escapedQuery = Regex.Escape(query);
        var queryFilter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Regex(u => u.Email, new BsonRegularExpression(escapedQuery, "i")),
            Builders<User>.Filter.Regex(u => u.Phone, new BsonRegularExpression(escapedQuery, "i"))
        );
        var filter = Builders<User>.Filter.And(activeFilter, queryFilter);

        return await Collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<bool> SoftDeleteAsync(string id, CancellationToken cancellationToken)
    {
        var objectId = GetValidObjectId(id);
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq("_id", objectId),
            Builders<User>.Filter.Eq(u => u.IsDeleted, false)
        );

        var updateDefinition = Builders<User>.Update
            .Set(u => u.IsDeleted, true)
            .Set(u => u.DeletedAt, DateTime.UtcNow)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await Collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancellationToken);
        return result.ModifiedCount > 0;
    }

    private static ObjectId GetValidObjectId(string id)
    {
        return !ObjectId.TryParse(id, out var objectId) ? throw new FormatException($"Invalid ObjectId format: {id}") : objectId;
    }
}
