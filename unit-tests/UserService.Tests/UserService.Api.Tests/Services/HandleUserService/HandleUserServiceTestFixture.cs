namespace UserService.Api.Tests.Services.HandleUserService;

using Moq;
using UserService.Api.Services;
using UserService.Data.Repositories;

public class HandleUserServiceTestFixture : IDisposable
{
    public Mock<IUserRepository> MockRepository { get; }
    public HandleUserService Service { get; }

    public HandleUserServiceTestFixture()
    {
        MockRepository = new Mock<IUserRepository>();
        Service = new HandleUserService(MockRepository.Object);
    }

    public void ResetMocks()
    {
        MockRepository.Reset();
    }

    public void Dispose()
    {
    }
}
