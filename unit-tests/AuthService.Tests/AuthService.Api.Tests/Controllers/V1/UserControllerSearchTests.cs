namespace AuthService.Api.Tests.Controllers.V1;

using AuthService.Api.Models;
using AuthService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

public class UserControllerSearchTests : IClassFixture<UserControllerTestFixture>
{
    private readonly UserControllerTestFixture _fixture;

    public UserControllerSearchTests(UserControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnOk()
    {
        var users = TestDataBuilder.CreateUserList(3);
        _fixture.MockRepository.Setup(x => x.SearchAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _fixture.Controller.Search(null, CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<List<UserDto>>(result);
        okResult.Should().HaveCount(3);
    }

    [Fact]
    public async Task Search_WithQuery_ShouldReturnOk()
    {
        var users = TestDataBuilder.CreateUserList(1);
        _fixture.MockRepository.Setup(x => x.SearchAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _fixture.Controller.Search("test", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<List<UserDto>>(result);
        okResult.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_WithQueryExceedingMaxLength_ShouldReturnBadRequest()
    {
        var longQuery = new string('a', 501);

        var result = await _fixture.Controller.Search(longQuery, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }
}
