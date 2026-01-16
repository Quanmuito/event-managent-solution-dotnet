namespace DatabaseService.Tests.Repositories;

using DatabaseService;
using DatabaseService.Exceptions;
using DatabaseService.Repositories;
using DatabaseService.Tests.Helpers;
using DatabaseService.Tests.Models;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class RepositoryTests
{
    private readonly Mock<MongoDbContext> _mockMongoDbContext;
    private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;
    private readonly Repository<TestEntity> _repository;

    public RepositoryTests()
    {
        (_mockMongoDbContext, _) = MongoDbContextTestHelper.SetupMongoDbContext();
        _mockCollection = new Mock<IMongoCollection<TestEntity>>(MockBehavior.Loose);
        _mockMongoDbContext.Setup(x => x.GetCollection<TestEntity>("TestCollection")).Returns(_mockCollection.Object);
        _repository = new Repository<TestEntity>(_mockMongoDbContext.Object, "TestCollection");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        var entityId = "507f1f77bcf86cd799439011";
        var expectedEntity = TestDataBuilder.CreateTestEntity(entityId);
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, expectedEntity);

        var result = await _repository.GetByIdAsync(entityId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(entityId);
        result.Title.Should().Be(expectedEntity.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var entityId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, null);

        var act = async () => await _repository.GetByIdAsync(entityId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"TestCollection with ID '{entityId}' was not found.");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";

        var act = async () => await _repository.GetByIdAsync(invalidId, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        var entities = new List<TestEntity>
        {
            TestDataBuilder.CreateTestEntity("507f1f77bcf86cd799439011"),
            TestDataBuilder.CreateTestEntity("507f1f77bcf86cd799439012"),
            TestDataBuilder.CreateTestEntity("507f1f77bcf86cd799439013")
        };
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, entities);

        var result = await _repository.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyCollection_ShouldReturnEmptyList()
    {
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, []);

        var result = await _repository.GetAllAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertAndReturnEntity()
    {
        var newEntity = TestDataBuilder.CreateTestEntity();
        _mockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<TestEntity>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _repository.CreateAsync(newEntity, CancellationToken.None);

        result.Should().Be(newEntity);
        _mockCollection.Verify(x => x.InsertOneAsync(
            It.Is<TestEntity>(e => e.Id == newEntity.Id && e.Title == newEntity.Title),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateAndReturnEntity()
    {
        var entityId = "507f1f77bcf86cd799439011";
        var updatedEntity = TestDataBuilder.CreateTestEntity(entityId);
        updatedEntity.Title = "Updated Title";
        var updateDefinition = Builders<TestEntity>.Update.Set(e => e.Title, "Updated Title");

        _mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<TestEntity>>(),
                It.IsAny<UpdateDefinition<TestEntity>>(),
                It.IsAny<FindOneAndUpdateOptions<TestEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        var result = await _repository.UpdateAsync(entityId, updateDefinition, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<UpdateDefinition<TestEntity>>(),
            It.IsAny<FindOneAndUpdateOptions<TestEntity>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var entityId = "507f1f77bcf86cd799439999";
        var updateDefinition = Builders<TestEntity>.Update.Set(e => e.Title, "Updated Title");

        _mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<TestEntity>>(),
                It.IsAny<UpdateDefinition<TestEntity>>(),
                It.IsAny<FindOneAndUpdateOptions<TestEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity?)null!);

        var act = async () => await _repository.UpdateAsync(entityId, updateDefinition, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"TestCollection with ID '{entityId}' was not found.");
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";
        var updateDefinition = Builders<TestEntity>.Update.Set(e => e.Title, "Updated Title");

        var act = async () => await _repository.UpdateAsync(invalidId, updateDefinition, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        var entityId = "507f1f77bcf86cd799439011";
        var deleteResult = new DeleteResult.Acknowledged(1);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<TestEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(entityId, CancellationToken.None);

        result.Should().BeTrue();
        _mockCollection.Verify(x => x.DeleteOneAsync(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var entityId = "507f1f77bcf86cd799439999";
        var deleteResult = new DeleteResult.Acknowledged(0);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<TestEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(entityId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidObjectId_ShouldThrowFormatException()
    {
        var invalidId = "invalid-id";

        var act = async () => await _repository.DeleteAsync(invalidId, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage($"Invalid ObjectId format: {invalidId}");
    }
}
