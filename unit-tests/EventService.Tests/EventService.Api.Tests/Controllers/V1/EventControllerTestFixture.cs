namespace EventService.Api.Tests.Controllers.V1;

using EventService.Api.Controllers.V1;
using EventService.Api.Services;
using EventService.Data.Repositories;
using Moq;

public class EventControllerTestFixture : IDisposable
{
    public Mock<IEventRepository> MockRepository { get; }
    public HandleEventService EventService { get; }
    public EventController Controller { get; }

    public EventControllerTestFixture()
    {
        MockRepository = new Mock<IEventRepository>();
        EventService = new HandleEventService(MockRepository.Object);
        Controller = new EventController(EventService);
    }

    public void ResetMocks()
    {
        MockRepository.Reset();
        Controller.ModelState.Clear();
    }

    public void Dispose()
    {
    }
}
