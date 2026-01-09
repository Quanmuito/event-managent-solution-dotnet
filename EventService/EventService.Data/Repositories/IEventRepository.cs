namespace EventService.Data.Repositories;

using EventService.Data.Models;

public interface IEventRepository : IRepository<Event>
{
    Task<List<Event>> SearchAsync(string? query, CancellationToken cancellationToken);
}
