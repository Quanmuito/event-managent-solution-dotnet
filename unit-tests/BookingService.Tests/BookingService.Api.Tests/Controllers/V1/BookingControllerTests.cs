namespace BookingService.Api.Tests.Controllers.V1;

using System.Collections.Generic;
using BookingService.Api.Controllers.V1;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Api.Messages;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using Ems.Common.Messages;
using Ems.Common.Services.Tasks;
using EventService.Data.Models;
using EventService.Data.Repositories;
using TestUtilities.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Moq;
using Xunit;

public class BookingControllerTests
{
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockQrCodeTaskQueue;
    private readonly Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>> _mockEmailNotificationTaskQueue;
    private readonly Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>> _mockPhoneNotificationTaskQueue;
    private readonly HandleBookingService _bookingService;
    private readonly BookingController _controller;

    public BookingControllerTests()
    {
        _mockRepository = new Mock<IBookingRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockQrCodeTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        _mockEmailNotificationTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>>();
        _mockPhoneNotificationTaskQueue = new Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>>();
        _bookingService = new HandleBookingService(
            _mockRepository.Object,
            _mockQrCodeRepository.Object,
            _mockEventRepository.Object,
            _mockQrCodeTaskQueue.Object,
            _mockEmailNotificationTaskQueue.Object,
            _mockPhoneNotificationTaskQueue.Object);
        _controller = new BookingController(_bookingService);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnOk()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        var bookingDto = new BookingDto(bookingEntity);
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(bookingDto);
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
    public async Task GetById_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _controller.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _controller.GetById("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task GetById_WithException_ShouldThrowException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var act = async () => await _controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _mockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = createDto.EventId });
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBooking);

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        createdAtResult!.ActionName.Should().Be(nameof(BookingController.GetById));
        createdAtResult.RouteValues!["id"].Should().Be(createdBooking.Id);
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
                _controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? string.Empty);
            }
        }

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        _controller.ModelState.AddModelError("Name", "Name is required");

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithArgumentException_ShouldReturnBadRequest()
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
                _controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? string.Empty);
            }
        }

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenEventDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        _mockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Events with ID '{createDto.EventId}' was not found."));

        var act = async () => await _controller.Create(createDto, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Events with ID '{createDto.EventId}' was not found.");
    }

    [Fact]
    public async Task Create_WithException_ShouldThrowException()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        _mockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventService.Data.Models.Event { Id = createDto.EventId });
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var act = async () => await _controller.Create(createDto, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldReturnOk()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        var updatedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        updatedBooking.Status = updateDto.Status!;
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
    }

    [Fact]
    public async Task Update_WithNullId_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();

        var result = await _controller.Update(null!, updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _controller.ModelState.AddModelError("Name", "Name is invalid");

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _controller.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _controller.Update("invalid-id", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Update_WithArgumentException_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("No valid fields to update."));

        var act = async () => await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Update_WithException_ShouldThrowException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var act = async () => await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Cancel_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _controller.Cancel(null!, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cancel_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _controller.Cancel("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Cancel_WithAlreadyCanceledBooking_ShouldThrowInvalidOperationException()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var act = async () => await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Booking is already canceled.");
    }

    [Fact]
    public async Task Cancel_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _controller.Cancel("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Cancel_WithException_ShouldThrowException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var act = async () => await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
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
    public async Task Delete_WithNonExistentId_ShouldReturn500()
    {
        _mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _controller.Delete("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Delete_WithException_ShouldThrowException()
    {
        _mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var act = async () => await _controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Confirm_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _controller.Confirm(null!, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Confirm_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _controller.Confirm("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Confirm_WithNonQueuePendingStatus_ShouldThrowInvalidOperationException()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var act = async () => await _controller.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only bookings with QueuePending status can be confirmed.");
    }

    [Fact]
    public async Task Confirm_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _controller.Confirm("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Confirm_WithException_ShouldThrowException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var act = async () => await _controller.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }
}
