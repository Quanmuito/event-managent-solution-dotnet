namespace UserService.Api.Tests.Controllers.V1;

using FluentAssertions;
using Moq;
using TestUtilities.Helpers;
using UserService.Api.Controllers.V1;
using UserService.Api.Models;
using UserService.Tests.Helpers;
using Xunit;

public class UserControllerCrudTests : IClassFixture<UserControllerTestFixture>
{
    private readonly UserControllerTestFixture _fixture;

    public UserControllerCrudTests(UserControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        var userEntity = TestDataBuilder.CreateDefaultUser();
        var userDto = new UserDto(userEntity);
        _fixture.MockRepository.Setup(x => x.GetActiveByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(userEntity);

        var result = await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<UserDto>(result);
        okResult.Should().BeEquivalentTo(userDto);
    }

    [Fact]
    public async Task GetById_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _fixture.Controller.GetById(null!, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task GetById_WithEmptyId_ShouldReturnBadRequest()
    {
        var result = await _fixture.Controller.GetById("   ", CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.GetActiveByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Users with ID '507f1f77bcf86cd799439999' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.GetById("507f1f77bcf86cd799439999", CancellationToken.None),
            "Users with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.GetActiveByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.GetById("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateUserDto();
        var createdUser = TestDataBuilder.CreateDefaultUser();
        ControllerTestSetupHelper.SetupMockRepositoryForCreate(_fixture.MockRepository, createdUser);

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertCreatedAtAction(result, nameof(UserController.GetById), createdUser.Id!);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateUserDto();
        _fixture.Controller.ModelState.AddModelError("Email", "Email is required");

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldReturnOk()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateUserDto();
        var updatedUser = TestDataBuilder.CreateDefaultUser();
        updatedUser.Email = updateDto.Email!;
        ControllerTestSetupHelper.SetupMockRepositoryForUpdate(_fixture.MockRepository, updatedUser, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertOkResult<UserDto>(result);
    }

    [Fact]
    public async Task Update_WithNullId_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateUserDto();

        var result = await _fixture.Controller.Update(null!, updateDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateUserDto();
        _fixture.Controller.ModelState.AddModelError("Email", "Email is invalid");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        _fixture.MockRepository.Setup(x => x.SoftDeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _fixture.Controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        ControllerTestHelper.AssertNoContent(result);
    }

    [Fact]
    public async Task Delete_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _fixture.Controller.Delete(null!, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturn500()
    {
        _fixture.MockRepository.Setup(x => x.SoftDeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _fixture.Controller.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.SoftDeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.Delete("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }
}
