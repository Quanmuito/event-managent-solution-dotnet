namespace TestUtilities.Helpers;

using System.Collections.Generic;
using DatabaseService;
using DatabaseService.Repositories;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public abstract class RepositoryTestBase<TEntity, TRepository>
    where TEntity : class
    where TRepository : IRepository<TEntity>
{
    protected readonly Mock<MongoDbContext> MockMongoDbContext;
    protected readonly Mock<IMongoCollection<TEntity>> MockCollection;
    protected readonly TRepository Repository;

    protected RepositoryTestBase()
    {
        (MockMongoDbContext, _) = MongoDbContextTestHelper.SetupMongoDbContext();
        MockCollection = new Mock<IMongoCollection<TEntity>>(MockBehavior.Loose);
        var collectionName = GetCollectionName();
        MockMongoDbContext.Setup(x => x.GetCollection<TEntity>(collectionName)).Returns(MockCollection.Object);
        Repository = CreateRepository(MockMongoDbContext.Object);
    }

    protected abstract string GetCollectionName();
    protected abstract TRepository CreateRepository(MongoDbContext mongoDbContext);
    protected abstract TEntity CreateEntity(string? id = null);
    protected abstract string GetValidEntityId();
    protected abstract string GetNonExistentEntityId();
    protected abstract UpdateDefinition<TEntity> CreateUpdateDefinition(TEntity entity);

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        var entityId = GetValidEntityId();
        var expectedEntity = CreateEntity(entityId);
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, expectedEntity);

        var result = await Repository.GetByIdAsync(entityId, CancellationToken.None);

        result.Should().NotBeNull();
        AssertEntityMatches(result, expectedEntity);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var entityId = GetNonExistentEntityId();
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, null);

        var act = async () => await Repository.GetByIdAsync(entityId, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"{GetCollectionName()} with ID '{entityId}' was not found.");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";

        var act = async () => await Repository.GetByIdAsync(invalidId, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        var entities = new List<TEntity>
        {
            CreateEntity(GetValidEntityId()),
            CreateEntity(GetValidEntityId()),
            CreateEntity(GetValidEntityId())
        };
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, entities);

        var result = await Repository.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyCollection_ShouldReturnEmptyList()
    {
        MongoDbMockHelper.SetupFindToListAsync(MockCollection, []);

        var result = await Repository.GetAllAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertAndReturnEntity()
    {
        var newEntity = CreateEntity();
        MockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<TEntity>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await Repository.CreateAsync(newEntity, CancellationToken.None);

        result.Should().Be(newEntity);
        MockCollection.Verify(x => x.InsertOneAsync(
            It.Is<TEntity>(e => AssertEntityEquals(e, newEntity)),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        var act = async () => await Repository.CreateAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("entity");
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateAndReturnEntity()
    {
        var entityId = GetValidEntityId();
        var updatedEntity = CreateEntity(entityId);
        var updateDefinition = CreateUpdateDefinition(updatedEntity);

        MockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<TEntity>>(),
                It.IsAny<UpdateDefinition<TEntity>>(),
                It.IsAny<FindOneAndUpdateOptions<TEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        var result = await Repository.UpdateAsync(entityId, updateDefinition, CancellationToken.None);

        result.Should().NotBeNull();
        AssertEntityMatches(result, updatedEntity);
        MockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<TEntity>>(),
            It.IsAny<UpdateDefinition<TEntity>>(),
            It.IsAny<FindOneAndUpdateOptions<TEntity>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var entityId = GetNonExistentEntityId();
        var updatedEntity = CreateEntity(entityId);
        var updateDefinition = CreateUpdateDefinition(updatedEntity);

        MockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<TEntity>>(),
                It.IsAny<UpdateDefinition<TEntity>>(),
                It.IsAny<FindOneAndUpdateOptions<TEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TEntity?)null!);

        var act = async () => await Repository.UpdateAsync(entityId, updateDefinition, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"{GetCollectionName()} with ID '{entityId}' was not found.");
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";
        var updatedEntity = CreateEntity();
        var updateDefinition = CreateUpdateDefinition(updatedEntity);

        var act = async () => await Repository.UpdateAsync(invalidId, updateDefinition, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        var entityId = GetValidEntityId();
        var deleteResult = new DeleteResult.Acknowledged(1);

        MockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<TEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await Repository.DeleteAsync(entityId, CancellationToken.None);

        result.Should().BeTrue();
        MockCollection.Verify(x => x.DeleteOneAsync(
            It.IsAny<FilterDefinition<TEntity>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var entityId = GetNonExistentEntityId();
        var deleteResult = new DeleteResult.Acknowledged(0);

        MockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<TEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await Repository.DeleteAsync(entityId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";

        var act = async () => await Repository.DeleteAsync(invalidId, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }

    protected virtual void AssertEntityMatches(TEntity actual, TEntity expected)
    {
        actual.Should().NotBeNull();
        expected.Should().NotBeNull();
    }

    protected virtual bool AssertEntityEquals(TEntity actual, TEntity expected)
    {
        return ReferenceEquals(actual, expected);
    }
}
