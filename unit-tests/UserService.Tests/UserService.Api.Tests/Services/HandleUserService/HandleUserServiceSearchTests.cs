namespace UserService.Api.Tests.Services.HandleUserService;

using FluentAssertions;
using Moq;
using UserService.Api.Models;
using UserService.Tests.Helpers;
using Xunit;

public class HandleUserServiceSearchTests : IClassFixture<HandleUserServiceTestFixture>
{
    private readonly HandleUserServiceTestFixture _fixture;

    public HandleUserServiceSearchTests(HandleUserServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnAllUsers()
    {
        var users = TestDataBuilder.CreateUserList(3);
        _fixture.MockRepository.Setup(x => x.SearchAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _fixture.Service.Search(null, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().AllBeOfType<UserDto>();
        _fixture.MockRepository.Verify(x => x.SearchAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnAllUsers()
    {
        var users = TestDataBuilder.CreateUserList(2);
        _fixture.MockRepository.Setup(x => x.SearchAsync("   ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _fixture.Service.Search("   ", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllBeOfType<UserDto>();
    }

    [Fact]
    public async Task Search_WithEmailQuery_ShouldFilterUsers()
    {
        var users = TestDataBuilder.CreateUserList(1);
        _fixture.MockRepository.Setup(x => x.SearchAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _fixture.Service.Search("test", CancellationToken.None);

        result.Should().HaveCount(1);
        result.Should().AllBeOfType<UserDto>();
        _fixture.MockRepository.Verify(x => x.SearchAsync("test", It.IsAny<CancellationToken>()), Times.Once);
    }
}
