namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Controllers.V1;
using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using EventService.Data.Models;
using TestUtilities.Helpers;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;

public class BookingControllerCrudTests : IClassFixture<BookingControllerTestFixture>
{
    private readonly BookingControllerTestFixture _fixture;

    public BookingControllerCrudTests(BookingControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        var bookingDto = new BookingDto(bookingEntity);
        ControllerTestSetupHelper.SetupMockRepositoryForGetById(_fixture.MockRepository, bookingEntity, "507f1f77bcf86cd799439011");
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<BookingDto>(result);
        okResult.Should().BeEquivalentTo(bookingDto);
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
        ControllerTestSetupHelper.SetupMockRepositoryForGetByIdThrows<IBookingRepository, Booking>(_fixture.MockRepository, new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.GetById("507f1f77bcf86cd799439999", CancellationToken.None),
            "Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForGetByIdThrows<IBookingRepository, Booking>(_fixture.MockRepository, new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.GetById("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task GetById_WithException_ShouldThrowException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForGetByIdThrows<IBookingRepository, Booking>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None),
            "Database error");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        var eventEntity = new Event { Id = createDto.EventId, Available = 10 };
        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);
        _fixture.MockEventRepository.Setup(x => x.OnBookingRegisteredAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);
        ControllerTestSetupHelper.SetupMockRepositoryForCreate(_fixture.MockRepository, createdBooking);

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertCreatedAtAction(result, nameof(BookingController.GetById), createdBooking.Id!);
    }

    [Fact]
    public async Task Create_WithInvalidStatus_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        createDto.Status = BookingStatus.Canceled;

        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(createDto);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(createDto, validationContext, validationResults, true);
        foreach (var validationResult in validationResults)
        {
            foreach (var memberName in validationResult.MemberNames)
            {
                _fixture.Controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? string.Empty);
            }
        }

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        _fixture.Controller.ModelState.AddModelError("Name", "Name is required");

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Create_WhenEventDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Events with ID '{createDto.EventId}' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.Create(createDto, CancellationToken.None),
            $"Events with ID '{createDto.EventId}' was not found.");
    }

    [Fact]
    public async Task Create_WithException_ShouldThrowException()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        var eventEntity = new Event { Id = createDto.EventId, Available = 10 };
        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);
        _fixture.MockEventRepository.Setup(x => x.OnBookingRegisteredAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);
        ControllerTestSetupHelper.SetupMockRepositoryForCreateThrows<IBookingRepository, Booking>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Create(createDto, CancellationToken.None),
            "Database error");
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldReturnOk()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        var updatedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        updatedBooking.Name = updateDto.Name!;
        ControllerTestSetupHelper.SetupMockRepositoryForUpdate(_fixture.MockRepository, updatedBooking, "507f1f77bcf86cd799439011");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertOkResult<BookingDto>(result);
    }

    [Fact]
    public async Task Update_WithNullId_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();

        var result = await _fixture.Controller.Update(null!, updateDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _fixture.Controller.ModelState.AddModelError("Name", "Name is invalid");

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IBookingRepository, Booking>(_fixture.MockRepository, new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None),
            "Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IBookingRepository, Booking>(_fixture.MockRepository, new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.Update("invalid-id", updateDto, CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Update_WithArgumentException_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateBookingDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IBookingRepository, Booking>(_fixture.MockRepository, new ArgumentException("No valid fields to update."));

        await ControllerTestHelper.AssertExceptionThrown<ArgumentException>(
            async () => await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None),
            "No valid fields to update.");
    }

    [Fact]
    public async Task Update_WithException_ShouldThrowException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        ControllerTestSetupHelper.SetupMockRepositoryForUpdateThrows<IBookingRepository, Booking>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None),
            "Database error");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        ControllerTestSetupHelper.SetupMockRepositoryForGetById(_fixture.MockRepository, booking, "507f1f77bcf86cd799439011");
        ControllerTestSetupHelper.SetupMockRepositoryForDelete<IBookingRepository, Booking>(_fixture.MockRepository, true);

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
        ControllerTestSetupHelper.SetupMockRepositoryForDelete<IBookingRepository, Booking>(_fixture.MockRepository, false);

        var result = await _fixture.Controller.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForDeleteThrows<IBookingRepository, Booking>(_fixture.MockRepository, new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.Delete("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Delete_WithException_ShouldThrowException()
    {
        ControllerTestSetupHelper.SetupMockRepositoryForDeleteThrows<IBookingRepository, Booking>(_fixture.MockRepository, new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None),
            "Database error");
    }
}
