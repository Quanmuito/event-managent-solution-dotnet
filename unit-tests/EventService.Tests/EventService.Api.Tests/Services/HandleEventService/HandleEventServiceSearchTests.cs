namespace EventService.Api.Tests.Services.HandleEventService;

using EventService.Api.Models;
using EventService.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

public class HandleEventServiceSearchTests : IClassFixture<HandleEventServiceTestFixture>
{
    private readonly HandleEventServiceTestFixture _fixture;

    public HandleEventServiceSearchTests(HandleEventServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnAllEvents()
    {
        var events = TestDataBuilder.CreateEventList(3);
        _fixture.MockRepository.Setup(x => x.SearchAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _fixture.Service.Search(null, CancellationToken.None);

        result.Should().HaveCount(3);
        result.Should().AllBeOfType<EventDto>();
        _fixture.MockRepository.Verify(x => x.SearchAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnAllEvents()
    {
        var events = TestDataBuilder.CreateEventList(2);
        _fixture.MockRepository.Setup(x => x.SearchAsync("   ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _fixture.Service.Search("   ", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllBeOfType<EventDto>();
    }

    [Fact]
    public async Task Search_WithSingleKeyword_ShouldFilterEvents()
    {
        var events = TestDataBuilder.CreateEventList(1);
        _fixture.MockRepository.Setup(x => x.SearchAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _fixture.Service.Search("test", CancellationToken.None);

        result.Should().HaveCount(1);
        result.Should().AllBeOfType<EventDto>();
        _fixture.MockRepository.Verify(x => x.SearchAsync("test", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithMultipleKeywords_ShouldFilterEvents()
    {
        var events = TestDataBuilder.CreateEventList(2);
        _fixture.MockRepository.Setup(x => x.SearchAsync("test, event", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _fixture.Service.Search("test, event", CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllBeOfType<EventDto>();
    }
}
