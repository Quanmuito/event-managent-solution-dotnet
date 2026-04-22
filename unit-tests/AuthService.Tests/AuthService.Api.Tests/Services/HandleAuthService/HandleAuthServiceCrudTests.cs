namespace AuthService.Api.Tests.Services.HandleAuthService;

using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;
using UserService.Data.Models;

public class HandleAuthServiceCrudTests : IClassFixture<HandleAuthServiceTestFixture>
{
    private readonly HandleAuthServiceTestFixture _fixture;

    public HandleAuthServiceCrudTests(HandleAuthServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnAuthDto()
    {
        var auth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");
        _fixture.MockAuthRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var result = await _fixture.Service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<AuthDto>();
        ServiceTestHelper.AssertDtoMatchesEntity(result, auth, "Id", "UserId", "PasswordHash", "Token", "CreatedAt", "UpdatedAt");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateAndReturnAuth()
    {
        var dto = TestDataBuilder.CreateValidCreateAuthDto();
        var createdAuth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011", dto.UserId, dto.Token);

        _fixture.MockAuthRepository.Setup(x => x.CreateAsync(It.IsAny<Auth>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Auth a, CancellationToken ct) => a);

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(dto.UserId);
        result.PasswordHash.Should().Be(dto.PasswordHash);
        result.Token.Should().Be(dto.Token);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _fixture.MockAuthRepository.Verify(x => x.CreateAsync(It.Is<Auth>(a =>
            a.UserId == dto.UserId &&
            a.PasswordHash == dto.PasswordHash &&
            a.Token == dto.Token), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldUpdateAndReturnAuth()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateAuthDto();
        var updatedAuth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");
        updatedAuth.PasswordHash = updateDto.PasswordHash!;
        updatedAuth.Token = updateDto.Token!;
        updatedAuth.UpdatedAt = DateTime.UtcNow;

        _fixture.MockAuthRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Auth>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedAuth);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.PasswordHash.Should().Be(updateDto.PasswordHash);
        result.Token.Should().Be(updateDto.Token);
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockAuthRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Auth>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNoValidFields_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateAuthDto();

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteAndReturnTrue()
    {
        _fixture.MockAuthRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _fixture.Service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeTrue();
        _fixture.MockAuthRepository.Verify(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithValidDto_ShouldCreateUserAndAuthAndReturnToken()
    {
        var registerDto = new RegisterDto
        {
            Email = "register@example.com",
            PasswordHash = "hashed-password-12345"
        };
        var createdUser = TestDataBuilder.CreateUser("507f1f77bcf86cd799439099", registerDto.Email, null);
        _fixture.MockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);
        _fixture.MockJwtTokenService.Setup(x => x.GenerateToken(createdUser.Id!, createdUser.Email, createdUser.Roles))
            .Returns("jwt-token-123");
        _fixture.MockAuthRepository.Setup(x => x.CreateAsync(It.IsAny<Auth>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Auth auth, CancellationToken _) => auth);

        var result = await _fixture.Service.Register(registerDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Message.Should().Be("Register success.");
        result.Token.Should().Be("jwt-token-123");
        _fixture.MockUserRepository.Verify(x => x.CreateAsync(It.Is<User>(u => u.Email == registerDto.Email), It.IsAny<CancellationToken>()), Times.Once);
        _fixture.MockJwtTokenService.Verify(x => x.GenerateToken(createdUser.Id!, createdUser.Email, createdUser.Roles), Times.Once);
        _fixture.MockAuthRepository.Verify(x => x.CreateAsync(It.Is<Auth>(a =>
            a.UserId == createdUser.Id &&
            a.PasswordHash == registerDto.PasswordHash &&
            a.Token == "jwt-token-123"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WhenUserCreateFails_ShouldThrowException()
    {
        var registerDto = new RegisterDto
        {
            Email = "register@example.com",
            PasswordHash = "hashed-password-12345"
        };
        _fixture.MockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot create user."));

        var act = async () => await _fixture.Service.Register(registerDto, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot create user.");
    }

    [Fact]
    public async Task Login_WithValidDto_ShouldReturnTokenAndPersistToken()
    {
        var loginDto = new LoginDto
        {
            Email = "login@example.com",
            PasswordHash = "hashed-password-12345"
        };
        var user = TestDataBuilder.CreateUser("507f1f77bcf86cd799439099", loginDto.Email, null);
        var auth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011", user.Id, "old-token");
        var refreshedToken = "jwt-token-456";
        _fixture.MockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _fixture.MockAuthRepository.Setup(x => x.GetByUserIdOrThrowAsync(user.Id!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);
        _fixture.MockJwtTokenService.Setup(x => x.GenerateToken(user.Id!, user.Email, user.Roles))
            .Returns(refreshedToken);
        _fixture.MockAuthRepository.Setup(x => x.UpdateAsync(auth.Id!, It.IsAny<UpdateDefinition<Auth>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var result = await _fixture.Service.Login(loginDto, CancellationToken.None);

        result.Should().Be(refreshedToken);
        _fixture.MockAuthRepository.Verify(x => x.UpdateAsync(auth.Id!, It.IsAny<UpdateDefinition<Auth>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        var loginDto = new LoginDto
        {
            Email = "login@example.com",
            PasswordHash = "wrong-password"
        };
        var user = TestDataBuilder.CreateUser("507f1f77bcf86cd799439099", loginDto.Email, null);
        var auth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011", user.Id, "old-token");
        _fixture.MockUserRepository.Setup(x => x.GetByEmailAsync(loginDto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _fixture.MockAuthRepository.Setup(x => x.GetByUserIdOrThrowAsync(user.Id!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var act = async () => await _fixture.Service.Login(loginDto, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }
}
