namespace AuthService.Api.Tests.Controllers.V1;

using AuthService.Api.Controllers.V1;
using AuthService.Api.Services;
using AuthService.Data.Repositories;
using Moq;

public class AuthControllerTestFixture : IDisposable
{
    public Mock<IAuthRepository> MockRepository { get; }
    public HandleAuthService AuthService { get; }
    public AuthController Controller { get; }

    public AuthControllerTestFixture()
    {
        MockRepository = new Mock<IAuthRepository>();
        AuthService = new HandleAuthService(MockRepository.Object);
        Controller = new AuthController(AuthService);
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
