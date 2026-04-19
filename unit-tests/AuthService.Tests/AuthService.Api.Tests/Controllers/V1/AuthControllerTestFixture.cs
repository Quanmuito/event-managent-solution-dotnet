namespace AuthService.Api.Tests.Controllers.V1;

using AuthService.Api.Controllers.V1;
using AuthService.Api.Services;
using AuthService.Data.Repositories;
using Moq;

public class AuthControllerTestFixture : IDisposable
{
    public Mock<IAuthRepository> MockAuthRepository { get; }
    public Mock<IUserRepository> MockUserRepository { get; }
    public Mock<IJwtTokenService> MockJwtTokenService { get; }
    public HandleAuthService AuthService { get; }
    public AuthController Controller { get; }

    public AuthControllerTestFixture()
    {
        MockAuthRepository = new Mock<IAuthRepository>();
        MockUserRepository = new Mock<IUserRepository>();
        MockJwtTokenService = new Mock<IJwtTokenService>();
        AuthService = new HandleAuthService(MockAuthRepository.Object, MockUserRepository.Object, MockJwtTokenService.Object);
        Controller = new AuthController(AuthService);
    }

    public void ResetMocks()
    {
        MockAuthRepository.Reset();
        MockUserRepository.Reset();
        MockJwtTokenService.Reset();
        Controller.ModelState.Clear();
    }

    public void Dispose()
    {
    }
}
