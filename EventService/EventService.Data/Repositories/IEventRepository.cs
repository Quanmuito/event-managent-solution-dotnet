namespace EventService.Data.Repositories;

using EventService.Data.Models;
using DatabaseService.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<List<Event>> SearchAsync(string? query, CancellationToken cancellationToken);
    Task<Event> OnBookingRegisteredAsync(string id, CancellationToken cancellationToken);
    Task<Event> OnBookingCancelledAsync(string id, CancellationToken cancellationToken);
}
