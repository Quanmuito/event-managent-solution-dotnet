namespace DatabaseService.Repositories;

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

public class Repository<T>(MongoDbContext mongoDbContext, string collectionName) : IRepository<T>
{
    protected readonly IMongoCollection<T> Collection = mongoDbContext.GetCollection<T>(collectionName);

    public async Task<T> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var objectId = GetValidObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        var result = await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"{collectionName} with ID '{id}' was not found.");

        return result;
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        return entity;
    }

    public async Task<T> UpdateAsync(string id, UpdateDefinition<T> updateDefinition, CancellationToken cancellationToken)
    {
        var objectId = GetValidObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        var options = new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.After };
        var result = await Collection.FindOneAndUpdateAsync(filter, updateDefinition, options, cancellationToken)
            ?? throw new KeyNotFoundException($"{collectionName} with ID '{id}' was not found.");

        return result;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var objectId = GetValidObjectId(id);
        var filter = Builders<T>.Filter.Eq("_id", objectId);
        var result = await Collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    private static ObjectId GetValidObjectId(string id)
    {
        return !ObjectId.TryParse(id, out var objectId) ? throw new FormatException($"Invalid ObjectId format: {id}") : objectId;
    }
}
