namespace AuthService.Api.Tests.Controllers.V1;

using AuthService.Api.Controllers.V1;
using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Data.Repositories;
using AuthService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
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
        ControllerTestSetupHelper.SetupMockRepositoryForGetById(_fixture.MockRepository, authEntity, "507f1f77bcf86cd799439011");

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
        ControllerTestSetupHelper.SetupMockRepositoryForCreate(_fixture.MockRepository, createdAuth);

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
        ControllerTestSetupHelper.SetupMockRepositoryForUpdate(_fixture.MockRepository, updatedAuth, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertOkResult<AuthDto>(result);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForDelete<IAuthRepository, Auth>(_fixture.MockRepository, true);

        var result = await _fixture.Controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        ControllerTestHelper.AssertNoContent(result);
    }
}
