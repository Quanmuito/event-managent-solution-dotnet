namespace AuthService.Api.Tests.Controllers.V1;

using AuthService.Api.Controllers.V1;
using AuthService.Api.Services;
using AuthService.Data.Repositories;
using Moq;

public class UserControllerTestFixture : IDisposable
{
    public Mock<IUserRepository> MockRepository { get; }
    public HandleUserService UserService { get; }
    public UserController Controller { get; }

    public UserControllerTestFixture()
    {
        MockRepository = new Mock<IUserRepository>();
        UserService = new HandleUserService(MockRepository.Object);
        Controller = new UserController(UserService);
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
