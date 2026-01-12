namespace EventService.Api.Tests.Controllers.V1;

using EventService.Api.Controllers.V1;
using EventService.Api.Models;
using EventService.Data.Models;
using EventService.Api.Services;
using EventService.Data.Repositories;
using EventService.Data.Exceptions;
using EventService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Xunit;

public class EventControllerTests
{
    private readonly Mock<ILogger<EventController>> _mockLogger;
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly HandleEventService _eventService;
    private readonly EventController _controller;

    public EventControllerTests()
    {
        _mockLogger = new Mock<ILogger<EventController>>();
        _mockRepository = new Mock<IEventRepository>();
        _eventService = new HandleEventService(_mockRepository.Object);
        _controller = new EventController(_mockLogger.Object, _eventService);
    }

    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnOk()
    {
        var events = TestDataBuilder.CreateEventList(2);
        var eventDtos = events.Select(e => new EventDto(e)).ToList();
        _mockRepository.Setup(x => x.SearchAsync("test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _controller.Search("test", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(eventDtos);
    }

    [Fact]
    public async Task Search_WithNullQuery_ShouldReturnOk()
    {
        var events = TestDataBuilder.CreateEventList(3);
        _mockRepository.Setup(x => x.SearchAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _controller.Search(null, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Search_WithQueryExceeding500Characters_ShouldReturnBadRequest()
    {
        var longQuery = new string('a', 501);

        var result = await _controller.Search(longQuery, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Search_WithException_ShouldReturn500()
    {
        _mockRepository.Setup(x => x.SearchAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Search("test", CancellationToken.None);

        AssertInternalServerError(result);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        var eventDto = new EventDto(eventEntity);
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        var result = await _controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(eventDto);
    }

    [Fact]
    public async Task GetById_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _controller.GetById(null!, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_WithEmptyId_ShouldReturnBadRequest()
    {
        var result = await _controller.GetById("   ", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldReturnNotFound()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Events", "507f1f77bcf86cd799439999"));

        var result = await _controller.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldReturnBadRequest()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var result = await _controller.GetById("invalid-id", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_WithException_ShouldReturn500()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        AssertInternalServerError(result);
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        var createdEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEvent);

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        createdAtResult!.ActionName.Should().Be(nameof(EventController.GetById));
        createdAtResult.RouteValues!["id"].Should().Be(createdEvent.Id);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        _controller.ModelState.AddModelError("Title", "Title is required");

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithArgumentException_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithException_ShouldReturn500()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Create(createDto, CancellationToken.None);

        AssertInternalServerError(result);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldReturnOk()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        var updatedEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        updatedEvent.Title = updateDto.Title!;
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEvent);

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<EventDto>();
    }

    [Fact]
    public async Task Update_WithNullId_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();

        var result = await _controller.Update(null!, updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _controller.ModelState.AddModelError("Title", "Title is invalid");

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldReturnNotFound()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Events", "507f1f77bcf86cd799439999"));

        var result = await _controller.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var result = await _controller.Update("invalid-id", updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithArgumentException_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("No valid fields to update."));

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithException_ShouldReturn500()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Event>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        AssertInternalServerError(result);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        _mockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _controller.Delete(null!, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnNotFound()
    {
        _mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldReturnBadRequest()
    {
        _mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var result = await _controller.Delete("invalid-id", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_WithException_ShouldReturn500()
    {
        _mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        AssertInternalServerError(result);
    }

    private static void AssertInternalServerError(IActionResult result)
    {
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
