namespace BookingService.Data.Tests.Repositories;

using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using DatabaseService;
using DatabaseService.Exceptions;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class BookingRepositoryTests
{
    private readonly Mock<MongoDbContext> _mockMongoDbContext;
    private readonly Mock<IMongoCollection<Booking>> _mockCollection;
    private readonly BookingRepository _repository;

    public BookingRepositoryTests()
    {
        (_mockMongoDbContext, _) = MongoDbContextTestHelper.SetupMongoDbContext();
        _mockCollection = new Mock<IMongoCollection<Booking>>(MockBehavior.Loose);
        _mockMongoDbContext.Setup(x => x.GetCollection<Booking>("Bookings")).Returns(_mockCollection.Object);
        _repository = new BookingRepository(_mockMongoDbContext.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnBooking()
    {
        var bookingId = "507f1f77bcf86cd799439011";
        var expectedBooking = TestDataBuilder.CreateBooking(bookingId);
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, expectedBooking);

        var result = await _repository.GetByIdAsync(bookingId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(bookingId);
        result.Status.Should().Be(expectedBooking.Status);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var bookingId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, null);

        var act = async () => await _repository.GetByIdAsync(bookingId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Bookings with ID '{bookingId}' was not found.");
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
    public async Task GetAllAsync_ShouldReturnAllBookings()
    {
        var bookings = TestDataBuilder.CreateBookingList(3);
        MongoDbMockHelper.SetupFindToListAsync(_mockCollection, bookings);

        var result = await _repository.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(bookings);
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertAndReturnBooking()
    {
        var newBooking = TestDataBuilder.CreateBooking();
        _mockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<Booking>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _repository.CreateAsync(newBooking, CancellationToken.None);

        result.Should().Be(newBooking);
        _mockCollection.Verify(x => x.InsertOneAsync(
            It.Is<Booking>(b => b.Id == newBooking.Id && b.Status == newBooking.Status),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateAndReturnBooking()
    {
        var bookingId = "507f1f77bcf86cd799439011";
        var updatedBooking = TestDataBuilder.CreateBooking(bookingId, BookingStatus.Canceled);
        updatedBooking.UpdatedAt = DateTime.UtcNow;
        var updateDefinition = Builders<Booking>.Update.Set(b => b.Status, BookingStatus.Canceled);

        _mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Booking>>(),
                It.IsAny<UpdateDefinition<Booking>>(),
                It.IsAny<FindOneAndUpdateOptions<Booking>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        var result = await _repository.UpdateAsync(bookingId, updateDefinition, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var bookingId = "507f1f77bcf86cd799439999";
        var updateDefinition = Builders<Booking>.Update.Set(b => b.Status, BookingStatus.Canceled);

        _mockCollection.Setup(x => x.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<Booking>>(),
                It.IsAny<UpdateDefinition<Booking>>(),
                It.IsAny<FindOneAndUpdateOptions<Booking>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null!);

        var act = async () => await _repository.UpdateAsync(bookingId, updateDefinition, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Bookings with ID '{bookingId}' was not found.");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        var bookingId = "507f1f77bcf86cd799439011";
        var deleteResult = new DeleteResult.Acknowledged(1);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Booking>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(bookingId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var bookingId = "507f1f77bcf86cd799439999";
        var deleteResult = new DeleteResult.Acknowledged(0);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Booking>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(bookingId, CancellationToken.None);

        result.Should().BeFalse();
    }
}
