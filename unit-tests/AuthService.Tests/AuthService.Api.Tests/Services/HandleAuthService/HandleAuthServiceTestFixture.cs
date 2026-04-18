namespace AuthService.Api.Tests.Services.HandleAuthService;

using AuthService.Api.Services;
using AuthService.Data.Repositories;
using Moq;

public class HandleAuthServiceTestFixture : IDisposable
{
    public Mock<IAuthRepository> MockRepository { get; }
    public HandleAuthService Service { get; }

    public HandleAuthServiceTestFixture()
    {
        MockRepository = new Mock<IAuthRepository>();
        Service = new HandleAuthService(MockRepository.Object);
    }

    public void ResetMocks()
    {
        MockRepository.Reset();
    }

    public void Dispose()
    {
    }
}
