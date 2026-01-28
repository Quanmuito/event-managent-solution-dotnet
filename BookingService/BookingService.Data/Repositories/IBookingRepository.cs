namespace BookingService.Data.Repositories;

using BookingService.Data.Models;
using DatabaseService.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking> ConfirmAsync(string id, CancellationToken cancellationToken);
    Task<Booking> CancelAsync(string id, CancellationToken cancellationToken);
}
