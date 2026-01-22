namespace BookingService.Data.Tests.Repositories;

using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Tests.Helpers;
using DatabaseService;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;

public class BookingRepositoryTests : RepositoryTestBase<Booking, BookingRepository>
{
    protected override string GetCollectionName()
    {
        return "Bookings";
    }

    protected override BookingRepository CreateRepository(MongoDbContext mongoDbContext)
    {
        return new BookingRepository(mongoDbContext);
    }

    protected override Booking CreateEntity(string? id = null)
    {
        return TestDataBuilder.CreateBooking(id);
    }

    protected override string GetValidEntityId()
    {
        return "507f1f77bcf86cd799439011";
    }

    protected override string GetNonExistentEntityId()
    {
        return "507f1f77bcf86cd799439999";
    }

    protected override UpdateDefinition<Booking> CreateUpdateDefinition(Booking entity)
    {
        return Builders<Booking>.Update.Set(b => b.Status, entity.Status);
    }

    protected override void AssertEntityMatches(Booking actual, Booking expected)
    {
        base.AssertEntityMatches(actual, expected);
        actual.Id.Should().Be(expected.Id);
        actual.Status.Should().Be(expected.Status);
    }

    protected override bool AssertEntityEquals(Booking actual, Booking expected)
    {
        return actual.Id == expected.Id && actual.Status == expected.Status;
    }
}
