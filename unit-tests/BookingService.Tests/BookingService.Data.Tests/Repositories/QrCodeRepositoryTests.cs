namespace BookingService.Data.Tests.Repositories;

using DatabaseService;
using DatabaseService.Exceptions;
using DatabaseService.Settings;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using TestUtilities.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

public class QrCodeRepositoryTests
{
    private readonly Mock<MongoDbContext> _mockMongoDbContext;
    private readonly Mock<IMongoCollection<QrCode>> _mockCollection;
    private readonly QrCodeRepository _repository;

    public QrCodeRepositoryTests()
    {
        var mockOptions = new Mock<IOptions<MongoDbSettings>>();
        mockOptions.Setup(x => x.Value).Returns(new MongoDbSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "test-db"
        });
        _mockMongoDbContext = new Mock<MongoDbContext>(mockOptions.Object) { CallBase = true };
        _mockCollection = new Mock<IMongoCollection<QrCode>>(MockBehavior.Loose);
        _mockMongoDbContext.Setup(x => x.GetCollection<QrCode>("QrCodes")).Returns(_mockCollection.Object);
        _repository = new QrCodeRepository(_mockMongoDbContext.Object);
    }

    [Fact]
    public async Task GetByBookingIdAsync_WithExistingBookingId_ShouldReturnQrCode()
    {
        var bookingId = "507f1f77bcf86cd799439011";
        var expectedQrCode = new QrCode
        {
            Id = "507f1f77bcf86cd799439012",
            BookingId = bookingId,
            QrCodeData = [1, 2, 3, 4, 5],
            CreatedAt = DateTime.UtcNow
        };
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, expectedQrCode);

        var result = await _repository.GetByBookingIdAsync(bookingId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.BookingId.Should().Be(bookingId);
        result.QrCodeData.Should().BeEquivalentTo(expectedQrCode.QrCodeData);
    }

    [Fact]
    public async Task GetByBookingIdAsync_WithNonExistentBookingId_ShouldReturnNull()
    {
        var bookingId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, null);

        var result = await _repository.GetByBookingIdAsync(bookingId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnQrCode()
    {
        var qrCodeId = "507f1f77bcf86cd799439011";
        var expectedQrCode = new QrCode
        {
            Id = qrCodeId,
            BookingId = "507f1f77bcf86cd799439012",
            QrCodeData = [1, 2, 3, 4, 5],
            CreatedAt = DateTime.UtcNow
        };
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, expectedQrCode);

        var result = await _repository.GetByIdAsync(qrCodeId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(qrCodeId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var qrCodeId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(_mockCollection, null);

        var act = async () => await _repository.GetByIdAsync(qrCodeId, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"QrCodes with ID '{qrCodeId}' was not found.");
    }

    [Fact]
    public async Task CreateAsync_ShouldInsertAndReturnQrCode()
    {
        var newQrCode = new QrCode
        {
            BookingId = "507f1f77bcf86cd799439011",
            QrCodeData = [1, 2, 3, 4, 5]
        };
        _mockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<QrCode>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _repository.CreateAsync(newQrCode, CancellationToken.None);

        result.Should().Be(newQrCode);
        _mockCollection.Verify(x => x.InsertOneAsync(
            It.Is<QrCode>(q => q.BookingId == newQrCode.BookingId),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        var qrCodeId = "507f1f77bcf86cd799439011";
        var deleteResult = new DeleteResult.Acknowledged(1);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<QrCode>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(qrCodeId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        var qrCodeId = "507f1f77bcf86cd799439999";
        var deleteResult = new DeleteResult.Acknowledged(0);

        _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<QrCode>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleteResult);

        var result = await _repository.DeleteAsync(qrCodeId, CancellationToken.None);

        result.Should().BeFalse();
    }
}
