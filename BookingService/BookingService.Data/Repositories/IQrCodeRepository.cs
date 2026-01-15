namespace BookingService.Data.Repositories;

using BookingService.Data.Models;
using DatabaseService.Repositories;

public interface IQrCodeRepository : IRepository<QrCode>
{
    Task<QrCode?> GetByBookingIdAsync(string bookingId, CancellationToken cancellationToken);
}
