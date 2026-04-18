namespace AuthService.Api.Tests.Controllers.V1;

using AuthService.Api.Models;
using AuthService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

public class AuthControllerSearchTests : IClassFixture<AuthControllerTestFixture>
{
    private readonly AuthControllerTestFixture _fixture;

    public AuthControllerSearchTests(AuthControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnOk()
    {
        var auths = TestDataBuilder.CreateAuthList(3);
        _fixture.MockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(auths);

        var result = await _fixture.Controller.Search(null, CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<List<AuthDto>>(result);
        okResult.Should().HaveCount(3);
    }
}
