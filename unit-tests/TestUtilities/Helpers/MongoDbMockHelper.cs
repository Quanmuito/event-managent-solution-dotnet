namespace TestUtilities.Helpers;

using MongoDB.Driver;
using Moq;

public static class MongoDbMockHelper
{
    public static void SetupFindToListAsync<T>(Mock<IMongoCollection<T>> mockCollection, List<T> data)
    {
        var cursor = CreateMockCursor(data);
        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<T>>(),
            It.IsAny<FindOptions<T, T>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor);
    }

    public static void SetupFindFirstOrDefaultAsync<T>(Mock<IMongoCollection<T>> mockCollection, T? result)
    {
        var data = result != null ? [result] : new List<T>();
        var cursor = CreateMockCursor(data);
        mockCollection.Setup(x => x.FindAsync(
            It.IsAny<FilterDefinition<T>>(),
            It.IsAny<FindOptions<T, T>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursor);
    }

    private static IAsyncCursor<T> CreateMockCursor<T>(List<T> data)
    {
        var mockCursor = new Mock<IAsyncCursor<T>>();
        var dataList = new List<T>(data);
        var hasMoved = false;

        mockCursor.Setup(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (hasMoved) return false;
                hasMoved = true;
                return dataList.Count > 0;
            });

        mockCursor.Setup(x => x.Current)
            .Returns(dataList);

        return mockCursor.Object;
    }
}
