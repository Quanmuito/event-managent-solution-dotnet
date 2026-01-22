namespace DatabaseService.Tests.Repositories;

using DatabaseService;
using DatabaseService.Repositories;
using DatabaseService.Tests.Helpers;
using DatabaseService.Tests.Models;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;

public class RepositoryTests : RepositoryTestBase<TestEntity, Repository<TestEntity>>
{
    protected override string GetCollectionName()
    {
        return "TestCollection";
    }

    protected override Repository<TestEntity> CreateRepository(MongoDbContext mongoDbContext)
    {
        return new Repository<TestEntity>(mongoDbContext, "TestCollection");
    }

    protected override TestEntity CreateEntity(string? id = null)
    {
        return TestDataBuilder.CreateTestEntity(id);
    }

    protected override string GetValidEntityId()
    {
        return "507f1f77bcf86cd799439011";
    }

    protected override string GetNonExistentEntityId()
    {
        return "507f1f77bcf86cd799439999";
    }

    protected override UpdateDefinition<TestEntity> CreateUpdateDefinition(TestEntity entity)
    {
        return Builders<TestEntity>.Update.Set(e => e.Title, entity.Title);
    }

    protected override void AssertEntityMatches(TestEntity actual, TestEntity expected)
    {
        base.AssertEntityMatches(actual, expected);
        actual.Id.Should().Be(expected.Id);
        actual.Title.Should().Be(expected.Title);
    }

    protected override bool AssertEntityEquals(TestEntity actual, TestEntity expected)
    {
        return actual.Id == expected.Id && actual.Title == expected.Title;
    }
}
