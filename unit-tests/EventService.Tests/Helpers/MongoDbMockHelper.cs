namespace EventService.Tests.Helpers;
using MongoDB.Driver;
using Moq;
using System.Linq.Expressions;

public static class MongoDbMockHelper
{
    public static Mock<IMongoCollection<T>> CreateMockCollection<T>(List<T> data)
    {
        var mockCollection = new Mock<IMongoCollection<T>>();
        var findFluent = new Mock<IFindFluent<T, T>>();

        findFluent.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        mockCollection.Setup(x => x.Find(
                It.IsAny<Expression<Func<T, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(findFluent.Object);

        mockCollection.Setup(x => x.Find(
                It.IsAny<FilterDefinition<T>>(),
                It.IsAny<FindOptions>()))
            .Returns(findFluent.Object);

        return mockCollection;
    }

    public static Mock<IMongoCollection<T>> CreateMockCollectionForFindOne<T>(T? result)
    {
        var mockCollection = new Mock<IMongoCollection<T>>();
        var findFluent = new Mock<IFindFluent<T, T>>();

        findFluent.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result!);

        mockCollection.Setup(x => x.Find(
                It.IsAny<Expression<Func<T, bool>>>(),
                It.IsAny<FindOptions>()))
            .Returns(findFluent.Object);

        return mockCollection;
    }

    public static Mock<IMongoCollection<T>> CreateMockCollectionForInsert<T>()
    {
        var mockCollection = new Mock<IMongoCollection<T>>();
        mockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<T>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mockCollection;
    }

    public static Mock<IMongoCollection<T>> CreateMockCollectionForUpdate<T>(T? result)
    {
        var mockCollection = new Mock<IMongoCollection<T>>();

        mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<Expression<Func<T, bool>>>(),
                It.IsAny<UpdateDefinition<T>>(),
                It.IsAny<FindOneAndUpdateOptions<T>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result!);

        return mockCollection;
    }

    public static Mock<IMongoCollection<T>> CreateMockCollectionForDelete<T>(long deletedCount)
    {
        var mockCollection = new Mock<IMongoCollection<T>>();
        var deleteResult = new DeleteResult.Acknowledged(deletedCount);

        mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<Expression<Func<T, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<T>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        return mockCollection;
    }
}
