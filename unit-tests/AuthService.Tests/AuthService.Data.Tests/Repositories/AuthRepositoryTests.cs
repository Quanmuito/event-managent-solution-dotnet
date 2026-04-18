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

public class AuthRepositoryTests : RepositoryTestBase<Auth, AuthRepository>
{
    protected override string GetCollectionName()
    {
        return "Auths";
    }

    protected override AuthRepository CreateRepository(MongoDbContext mongoDbContext)
    {
        return new AuthRepository(mongoDbContext);
    }

    protected override Auth CreateEntity(string? id = null)
    {
        return TestDataBuilder.CreateAuth(id);
    }

    protected override string GetValidEntityId()
    {
        return "507f1f77bcf86cd799439011";
    }

    protected override string GetNonExistentEntityId()
    {
        return "507f1f77bcf86cd799439999";
    }

    protected override UpdateDefinition<Auth> CreateUpdateDefinition(Auth entity)
    {
        return Builders<Auth>.Update.Set(a => a.Token, entity.Token);
    }

    protected override void AssertEntityMatches(Auth actual, Auth expected)
    {
        base.AssertEntityMatches(actual, expected);
        actual.Id.Should().Be(expected.Id);
        actual.UserId.Should().Be(expected.UserId);
        actual.PasswordHash.Should().Be(expected.PasswordHash);
        actual.Token.Should().Be(expected.Token);
    }

    protected override bool AssertEntityEquals(Auth actual, Auth expected)
    {
        return actual.Id == expected.Id &&
               actual.UserId == expected.UserId &&
               actual.PasswordHash == expected.PasswordHash &&
               actual.Token == expected.Token;
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnAllAuthsForUser()
    {
        var userId = "507f1f77bcf86cd799439011";
        var auths = new List<Auth>
        {
            TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011", userId, "token1"),
            TestDataBuilder.CreateAuth("507f1f77bcf86cd799439012", userId, "token2")
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, auths);

        var result = await Repository.GetByUserIdAsync(userId, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.UserId.Should().Be(userId));
        MockCollection.Verify(x => x.FindAsync(
            It.IsAny<FilterDefinition<Auth>>(),
            It.IsAny<FindOptions<Auth, Auth>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNonExistentUserId_ShouldReturnEmptyList()
    {
        var userId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, []);

        var result = await Repository.GetByUserIdAsync(userId, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
