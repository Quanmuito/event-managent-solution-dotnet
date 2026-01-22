namespace EventService.Api.Tests.Services.HandleEventService;

using EventService.Api.Services;
using EventService.Data.Repositories;
using Moq;

public class HandleEventServiceTestFixture : IDisposable
{
    public Mock<IEventRepository> MockRepository { get; }
    public HandleEventService Service { get; }

    public HandleEventServiceTestFixture()
    {
        MockRepository = new Mock<IEventRepository>();
        Service = new HandleEventService(MockRepository.Object);
    }

    public void ResetMocks()
    {
        MockRepository.Reset();
    }

    public void Dispose()
    {
    }
}
