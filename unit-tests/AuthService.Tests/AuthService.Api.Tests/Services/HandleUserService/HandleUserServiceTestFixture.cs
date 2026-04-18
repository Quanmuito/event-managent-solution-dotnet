namespace AuthService.Api.Tests.Services.HandleUserService;

using AuthService.Api.Services;
using AuthService.Data.Repositories;
using Moq;

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
