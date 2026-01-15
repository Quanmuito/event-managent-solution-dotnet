namespace BookingService.Data.Repositories;

using BookingService.Data.Models;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Driver;

public class QrCodeRepository(MongoDbContext mongoDbContext) : Repository<QrCode>(mongoDbContext, "QrCodes"), IQrCodeRepository
{
    public async Task<QrCode?> GetByBookingIdAsync(string bookingId, CancellationToken cancellationToken)
    {
        var filter = Builders<QrCode>.Filter.Eq(q => q.BookingId, bookingId);
        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
