namespace AuthService.Api.Tests.Controllers.V1;

using AuthService.Api.Controllers.V1;
using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Data.Repositories;
using AuthService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using UserService.Data.Models;
using Xunit;

public class AuthControllerCrudTests : IClassFixture<AuthControllerTestFixture>
{
    private readonly AuthControllerTestFixture _fixture;

    public AuthControllerCrudTests(AuthControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        var authEntity = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");
        var authDto = new AuthDto(authEntity);
        ControllerTestSetupHelper.SetupMockRepositoryForGetById(_fixture.MockAuthRepository, authEntity, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<AuthDto>(result);
        okResult.Should().BeEquivalentTo(authDto);
    }

    [Fact]
    public async Task GetById_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _fixture.Controller.GetById(null!, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateAuthDto();
        var createdAuth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011", createDto.UserId, createDto.Token);
        ControllerTestSetupHelper.SetupMockRepositoryForCreate(_fixture.MockAuthRepository, createdAuth);

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertCreatedAtAction(result, nameof(AuthController.GetById), createdAuth.Id!);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateAuthDto();
        _fixture.Controller.ModelState.AddModelError("UserId", "UserId is required");

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldReturnOk()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateAuthDto();
        var updatedAuth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");
        updatedAuth.Token = updateDto.Token!;
        ControllerTestSetupHelper.SetupMockRepositoryForUpdate(_fixture.MockAuthRepository, updatedAuth, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertOkResult<AuthDto>(result);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForDelete<IAuthRepository, Auth>(_fixture.MockAuthRepository, true);

        var result = await _fixture.Controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        ControllerTestHelper.AssertNoContent(result);
    }

    [Fact]
    public async Task Register_WithValidDto_ShouldReturnOkWithToken()
    {
        var registerDto = new RegisterDto
        {
            Email = "register@example.com",
            PasswordHash = "hashed-password-12345"
        };
        var createdUser = TestDataBuilder.CreateUser("507f1f77bcf86cd799439099", registerDto.Email, null);
        _fixture.MockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);
        _fixture.MockJwtTokenService.Setup(x => x.GenerateToken(createdUser.Id!, createdUser.Email))
            .Returns("jwt-token-123");
        _fixture.MockAuthRepository.Setup(x => x.CreateAsync(It.IsAny<Auth>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Auth auth, CancellationToken _) => auth);

        var result = await _fixture.Controller.Register(registerDto, CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<RegisterResultDto>(result);
        okResult.Message.Should().Be("Register success.");
        okResult.Token.Should().Be("jwt-token-123");
    }

    [Fact]
    public async Task Register_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var registerDto = new RegisterDto
        {
            Email = "register@example.com",
            PasswordHash = "hashed-password-12345"
        };
        _fixture.Controller.ModelState.AddModelError("Email", "Email is required");

        var result = await _fixture.Controller.Register(registerDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Login_WithValidDto_ShouldReturnOkWithToken()
    {
        var loginDto = new LoginDto
        {
            Email = "login@example.com",
            PasswordHash = "hashed-password-12345"
        };
        var user = TestDataBuilder.CreateUser("507f1f77bcf86cd799439099", loginDto.Email, null);
        var auth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011", user.Id, "old-token");
        _fixture.MockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _fixture.MockAuthRepository.Setup(x => x.GetByUserIdOrThrowAsync(user.Id!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);
        _fixture.MockJwtTokenService.Setup(x => x.GenerateToken(user.Id!, user.Email))
            .Returns("jwt-token-123");
        _fixture.MockAuthRepository.Setup(x => x.UpdateAsync(auth.Id!, It.IsAny<UpdateDefinition<Auth>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var result = await _fixture.Controller.Login(loginDto, CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<string>(result);
        okResult.Should().Be("jwt-token-123");
    }

    [Fact]
    public async Task Login_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var loginDto = new LoginDto
        {
            Email = "login@example.com",
            PasswordHash = "hashed-password-12345"
        };
        _fixture.Controller.ModelState.AddModelError("Email", "Email is required");

        var result = await _fixture.Controller.Login(loginDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Register_WhenServiceThrowsInvalidOperation_ShouldThrow()
    {
        var registerDto = new RegisterDto
        {
            Email = "register@example.com",
            PasswordHash = "hashed-password-12345"
        };
        _fixture.MockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Registration failed."));

        var act = async () => await _fixture.Controller.Register(registerDto, CancellationToken.None);
        await ControllerTestHelper.AssertExceptionThrown<InvalidOperationException>(act, "Registration failed.");
    }

    [Fact]
    public async Task Register_WhenServiceThrowsUnexpectedException_ShouldThrow()
    {
        var registerDto = new RegisterDto
        {
            Email = "register@example.com",
            PasswordHash = "hashed-password-12345"
        };
        _fixture.MockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected"));

        var act = async () => await _fixture.Controller.Register(registerDto, CancellationToken.None);
        await ControllerTestHelper.AssertExceptionThrown<Exception>(act, "Unexpected");
    }
}
