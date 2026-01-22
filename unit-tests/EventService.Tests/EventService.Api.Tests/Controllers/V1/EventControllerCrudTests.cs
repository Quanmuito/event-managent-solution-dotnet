namespace EventService.Api.Tests.Controllers.V1;

using EventService.Api.Controllers.V1;
using EventService.Api.Models;
using EventService.Data.Models;
using EventService.Data.Repositories;
using EventService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Xunit;

public class EventControllerCrudTests : IClassFixture<EventControllerTestFixture>
{
    private readonly EventControllerTestFixture _fixture;

    public EventControllerCrudTests(EventControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        var eventEntity = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        var eventDto = new EventDto(eventEntity);
        ControllerTestSetupHelper.SetupMockRepositoryForGetById(_fixture.MockRepository, eventEntity, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<EventDto>(result);
        okResult.Should().BeEquivalentTo(eventDto);
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
        ControllerTestSetupHelper.SetupMockRepositoryForGetByIdThrows<IEventRepository, Event>(_fixture.MockRepository, new KeyNotFoundException("Events with ID '507f1f77bcf86cd799439999' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.GetById("507f1f77bcf86cd799439999", CancellationToken.None),
            "Events with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForGetByIdThrows<IEventRepository, Event>(_fixture.MockRepository, new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.GetById("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task GetById_WithException_ShouldThrowException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForGetByIdThrows<IEventRepository, Event>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None),
            "Database error");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        var createdEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        ControllerTestSetupHelper.SetupMockRepositoryForCreate(_fixture.MockRepository, createdEvent);

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertCreatedAtAction(result, nameof(EventController.GetById), createdEvent.Id!);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        _fixture.Controller.ModelState.AddModelError("Title", "Title is required");

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Create_WithArgumentException_ShouldThrowArgumentException()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        ControllerTestSetupHelper.SetupMockRepositoryForCreateThrows<IEventRepository, Event>(_fixture.MockRepository, new ArgumentException("Invalid argument"));

        await ControllerTestHelper.AssertExceptionThrown<ArgumentException>(
            async () => await _fixture.Controller.Create(createDto, CancellationToken.None),
            "Invalid argument");
    }

    [Fact]
    public async Task Create_WithException_ShouldThrowException()
    {
        var createDto = TestDataBuilder.CreateValidCreateEventDto();
        ControllerTestSetupHelper.SetupMockRepositoryForCreateThrows<IEventRepository, Event>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Create(createDto, CancellationToken.None),
            "Database error");
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldReturnOk()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        var updatedEvent = TestDataBuilder.CreateEvent("507f1f77bcf86cd799439011");
        updatedEvent.Title = updateDto.Title!;
        ControllerTestSetupHelper.SetupMockRepositoryForUpdate(_fixture.MockRepository, updatedEvent, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertOkResult<EventDto>(result);
    }

    [Fact]
    public async Task Update_WithNullId_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();

        var result = await _fixture.Controller.Update(null!, updateDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        _fixture.Controller.ModelState.AddModelError("Title", "Title is invalid");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IEventRepository, Event>(_fixture.MockRepository, new KeyNotFoundException("Events with ID '507f1f77bcf86cd799439999' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None),
            "Events with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IEventRepository, Event>(_fixture.MockRepository, new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.Update("invalid-id", updateDto, CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Update_WithArgumentException_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateEventDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IEventRepository, Event>(_fixture.MockRepository, new ArgumentException("No valid fields to update."));

        await ControllerTestHelper.AssertExceptionThrown<ArgumentException>(
            async () => await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None),
            "No valid fields to update.");
    }

    [Fact]
    public async Task Update_WithException_ShouldThrowException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateEventDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IEventRepository, Event>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None),
            "Database error");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForDelete<IEventRepository, Event>(_fixture.MockRepository, true);

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
        ControllerTestSetupHelper.SetupMockRepositoryForDelete<IEventRepository, Event>(_fixture.MockRepository, false);

        var result = await _fixture.Controller.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForDeleteThrows<IEventRepository, Event>(_fixture.MockRepository, new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.Delete("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Delete_WithException_ShouldThrowException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForDeleteThrows<IEventRepository, Event>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None),
            "Database error");
    }
}
