namespace BookingService.Data.Tests.Repositories;

using BookingService.Data.Models;
using BookingService.Data.Repositories;
using DatabaseService;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

public class QrCodeRepositoryTests : RepositoryTestBase<QrCode, QrCodeRepository>
{
    protected override string GetCollectionName()
    {
        return "QrCodes";
    }

    protected override QrCodeRepository CreateRepository(MongoDbContext mongoDbContext)
    {
        return new QrCodeRepository(mongoDbContext);
    }

    protected override QrCode CreateEntity(string? id = null)
    {
        return new QrCode
        {
            Id = id ?? "507f1f77bcf86cd799439011",
            BookingId = "507f1f77bcf86cd799439012",
            QrCodeData = [1, 2, 3, 4, 5],
            CreatedAt = DateTime.UtcNow
        };
    }

    protected override string GetValidEntityId()
    {
        return "507f1f77bcf86cd799439011";
    }

    protected override string GetNonExistentEntityId()
    {
        return "507f1f77bcf86cd799439999";
    }

    protected override UpdateDefinition<QrCode> CreateUpdateDefinition(QrCode entity)
    {
        return Builders<QrCode>.Update.Set(q => q.BookingId, entity.BookingId);
    }

    protected override void AssertEntityMatches(QrCode actual, QrCode expected)
    {
        base.AssertEntityMatches(actual, expected);
        actual.Id.Should().Be(expected.Id);
        actual.BookingId.Should().Be(expected.BookingId);
    }

    protected override bool AssertEntityEquals(QrCode actual, QrCode expected)
    {
        return actual.Id == expected.Id && actual.BookingId == expected.BookingId;
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
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, expectedQrCode);

        var result = await Repository.GetByBookingIdAsync(bookingId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.BookingId.Should().Be(bookingId);
        result.QrCodeData.Should().BeEquivalentTo(expectedQrCode.QrCodeData);
    }

    [Fact]
    public async Task GetByBookingIdAsync_WithNonExistentBookingId_ShouldReturnNull()
    {
        var bookingId = "507f1f77bcf86cd799439999";
        MongoDbMockHelper.SetupFindFirstOrDefaultAsync(MockCollection, null);

        var result = await Repository.GetByBookingIdAsync(bookingId, CancellationToken.None);

        result.Should().BeNull();
    }
}
