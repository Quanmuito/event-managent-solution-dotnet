namespace UserService.Api.Tests.Controllers.V1;

using Moq;
using UserService.Api.Controllers.V1;
using UserService.Api.Services;
using UserService.Data.Repositories;

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
