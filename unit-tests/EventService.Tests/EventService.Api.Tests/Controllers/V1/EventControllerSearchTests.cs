namespace EventService.Api.Tests.Controllers.V1;

using EventService.Api.Models;
using EventService.Tests.Helpers;
using TestUtilities.Helpers;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;

public class EventControllerSearchTests : IClassFixture<EventControllerTestFixture>
{
    private readonly EventControllerTestFixture _fixture;

    public EventControllerSearchTests(EventControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnOk()
    {
        var events = TestDataBuilder.CreateEventList(2);
        var eventDtos = events.Select(e => new EventDto(e)).ToList();
        _fixture.MockRepository.Setup(x => x.SearchAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _fixture.Controller.Search("test", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<List<EventDto>>(result);
        okResult.Should().BeEquivalentTo(eventDtos);
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnOk()
    {
        var events = TestDataBuilder.CreateEventList(3);
        _fixture.MockRepository.Setup(x => x.SearchAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _fixture.Controller.Search(null, CancellationToken.None);

        ControllerTestHelper.AssertOkResult<List<EventDto>>(result);
    }

    [Fact]
    public async Task Search_WithQueryExceeding500Characters_ShouldReturnBadRequest()
    {
        var longQuery = new string('a', 501);

        var result = await _fixture.Controller.Search(longQuery, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Search_WithException_ShouldThrowException()
    {
        _fixture.MockRepository.Setup(x => x.SearchAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Search("test", CancellationToken.None),
            "Database error");
    }
}
