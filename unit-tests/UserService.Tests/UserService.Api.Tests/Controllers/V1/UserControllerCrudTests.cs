namespace UserService.Api.Tests.Controllers.V1;

using FluentAssertions;
using TestUtilities.Helpers;
using UserService.Api.Controllers.V1;
using UserService.Api.Models;
using UserService.Data.Models;
using UserService.Data.Repositories;
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
        ControllerTestSetupHelper.SetupMockRepositoryForGetById(_fixture.MockRepository, userEntity, "507f1f77bcf86cd799439011");

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
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        var user = TestDataBuilder.CreateDefaultUser();
        user.IsDeleted = true;
        ControllerTestSetupHelper.SetupMockRepositoryForUpdate(_fixture.MockRepository, user, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        ControllerTestHelper.AssertNoContent(result);
    }

    [Fact]
    public async Task Delete_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _fixture.Controller.Delete(null!, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }
}
