namespace AuthService.Api.Tests.Services.HandleAuthService;

using AuthService.Api.Services;
using AuthService.Data.Repositories;
using Moq;

public class HandleAuthServiceTestFixture : IDisposable
{
    public Mock<IAuthRepository> MockAuthRepository { get; }
    public Mock<IUserRepository> MockUserRepository { get; }
    public Mock<IJwtTokenService> MockJwtTokenService { get; }
    public HandleAuthService Service { get; }

    public HandleAuthServiceTestFixture()
    {
        MockAuthRepository = new Mock<IAuthRepository>();
        MockUserRepository = new Mock<IUserRepository>();
        MockJwtTokenService = new Mock<IJwtTokenService>();
        Service = new HandleAuthService(MockAuthRepository.Object, MockUserRepository.Object, MockJwtTokenService.Object);
    }

    public void ResetMocks()
    {
        MockAuthRepository.Reset();
        MockUserRepository.Reset();
        MockJwtTokenService.Reset();
    }

    public void Dispose()
    {
    }
}
