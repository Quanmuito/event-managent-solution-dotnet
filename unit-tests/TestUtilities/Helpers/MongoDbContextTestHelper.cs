namespace TestUtilities.Helpers;

using DatabaseService;
using DatabaseService.Settings;
using Microsoft.Extensions.Options;
using Moq;

public static class MongoDbContextTestHelper
{
    public static (Mock<MongoDbContext> mockMongoDbContext, Mock<IOptions<MongoDbSettings>> mockOptions) SetupMongoDbContext()
    {
        var mockOptions = new Mock<IOptions<MongoDbSettings>>();
        mockOptions.Setup(x => x.Value).Returns(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        });
        var mockMongoDbContext = new Mock<MongoDbContext>(mockOptions.Object) { CallBase = true };

        return (mockMongoDbContext, mockOptions);
    }
}
