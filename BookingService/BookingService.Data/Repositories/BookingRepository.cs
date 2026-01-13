namespace BookingService.Data.Repositories;

using BookingService.Data.Models;
using DatabaseService;
using DatabaseService.Repositories;

public class BookingRepository(MongoDbContext mongoDbContext) : Repository<Booking>(mongoDbContext, "Bookings"), IBookingRepository
{
}
