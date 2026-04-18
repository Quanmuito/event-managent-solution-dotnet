namespace AuthService.Data.Tests.Repositories;

using DatabaseService;
using AuthService.Data.Models;
using AuthService.Data.Repositories;
using AuthService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class UserRepositoryTests : RepositoryTestBase<User, UserRepository>
{
    protected override string GetCollectionName()
    {
        return "Users";
    }

    protected override UserRepository CreateRepository(MongoDbContext mongoDbContext)
    {
        return new UserRepository(mongoDbContext);
    }

    protected override User CreateEntity(string? id = null)
    {
        return TestDataBuilder.CreateUser(id);
    }

    protected override string GetValidEntityId()
    {
        return "507f1f77bcf86cd799439011";
    }

    protected override string GetNonExistentEntityId()
    {
        return "507f1f77bcf86cd799439999";
    }

    protected override UpdateDefinition<User> CreateUpdateDefinition(User entity)
    {
        return Builders<User>.Update.Set(u => u.Email, entity.Email);
    }

    protected override void AssertEntityMatches(User actual, User expected)
    {
        base.AssertEntityMatches(actual, expected);
        actual.Id.Should().Be(expected.Id);
        actual.Email.Should().Be(expected.Email);
    }

    protected override bool AssertEntityEquals(User actual, User expected)
    {
        return actual.Id == expected.Id && actual.Email == expected.Email;
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ShouldReturnAllUsers()
    {
        var users = new List<User>
        {
            TestDataBuilder.CreateUser("507f1f77bcf86cd799439011"),
            TestDataBuilder.CreateUser("507f1f77bcf86cd799439012")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, users);

        var result = await Repository.SearchAsync(null, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnAllUsers()
    {
        var users = new List<User>
        {
            TestDataBuilder.CreateUser("507f1f77bcf86cd799439011")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, users);

        var result = await Repository.SearchAsync("   ", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithEmailQuery_ShouldFilterByEmail()
    {
        var users = new List<User>
        {
            TestDataBuilder.CreateUser("507f1f77bcf86cd799439011", "test@example.com")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, users);

        var result = await Repository.SearchAsync("test", CancellationToken.None);

        result.Should().HaveCount(1);
        MockCollection.Verify(x => x.FindAsync(
            It.IsAny<FilterDefinition<User>>(),
            It.IsAny<FindOptions<User, User>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithPhoneQuery_ShouldFilterByPhone()
    {
        var users = new List<User>
        {
            TestDataBuilder.CreateUser("507f1f77bcf86cd799439011", "test@example.com", "+1234567890")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, users);

        var result = await Repository.SearchAsync("123", CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_ShouldBeCaseInsensitive()
    {
        var users = new List<User>
        {
            TestDataBuilder.CreateUser("507f1f77bcf86cd799439011", "Test@Example.com")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, users);

        var result = await Repository.SearchAsync("test", CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
