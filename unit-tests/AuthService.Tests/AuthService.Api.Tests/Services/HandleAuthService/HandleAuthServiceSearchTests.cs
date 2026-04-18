namespace AuthService.Api.Tests.Services.HandleAuthService;

using AuthService.Api.Models;
using AuthService.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

public class HandleAuthServiceSearchTests : IClassFixture<HandleAuthServiceTestFixture>
{
    private readonly HandleAuthServiceTestFixture _fixture;

    public HandleAuthServiceSearchTests(HandleAuthServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnAllAuths()
    {
        var auths = TestDataBuilder.CreateAuthList(3);
        _fixture.MockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(auths);

        var result = await _fixture.Service.Search(null, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().AllBeOfType<AuthDto>();
        _fixture.MockRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnAllAuths()
    {
        var auths = TestDataBuilder.CreateAuthList(2);
        _fixture.MockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(auths);

        var result = await _fixture.Service.Search("   ", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllBeOfType<AuthDto>();
    }
}
