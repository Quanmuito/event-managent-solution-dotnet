namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Controllers.V1;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using DatabaseService.Exceptions;
using Ems.Common.Services.Tasks.Messages;
using Ems.Common.Services.Tasks;
using TestUtilities.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Xunit;

public class BookingControllerTests
{
    private readonly Mock<ILogger<BookingController>> _mockLogger;
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockTaskQueue;
    private readonly Mock<ITaskQueue<EmailNotificationTaskMessage>> _mockEmailTaskQueue;
    private readonly HandleBookingService _bookingService;
    private readonly BookingController _controller;

    public BookingControllerTests()
    {
        _mockLogger = new Mock<ILogger<BookingController>>();
        _mockRepository = new Mock<IBookingRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        _mockEmailTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage>>();
        _bookingService = new HandleBookingService(_mockRepository.Object, _mockQrCodeRepository.Object, _mockTaskQueue.Object, _mockEmailTaskQueue.Object);
        _controller = new BookingController(_mockLogger.Object, _bookingService);
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
    public async Task GetById_WithNonExistentId_ShouldReturnNotFound()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

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

        ControllerTestHelper.AssertInternalServerError(result);
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBooking);

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        createdAtResult!.ActionName.Should().Be(nameof(BookingController.GetById));
        createdAtResult.RouteValues!["id"].Should().Be(createdBooking.Id);
    }

    [Fact]
    public async Task Create_WithRegisteredStatus_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        createDto.Status = BookingStatus.Registered;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = BookingStatus.Registered;
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBooking);

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_WithQueueEnrolledStatus_ShouldReturnCreatedAtAction()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        createDto.Status = BookingStatus.QueueEnrolled;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = BookingStatus.QueueEnrolled;
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdBooking);

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
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
    public async Task Create_WithException_ShouldReturn500()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Create(createDto, CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
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
    public async Task Update_WithNonExistentId_ShouldReturnNotFound()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

        var result = await _controller.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldReturnBadRequest()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var result = await _controller.Update("invalid-id", updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithArgumentException_ShouldReturnBadRequest()
    {
        var updateDto = new UpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("No valid fields to update."));

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WithException_ShouldReturn500()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
    }

    [Fact]
    public async Task Cancel_WithValidId_ShouldReturnOk()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
    }

    [Fact]
    public async Task Cancel_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _controller.Cancel(null!, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cancel_WithNonExistentId_ShouldReturnNotFound()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

        var result = await _controller.Cancel("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cancel_WithAlreadyCanceledBooking_ShouldReturnBadRequest()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cancel_WithQueueEnrolledStatus_ShouldReturnOk()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueueEnrolled);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
        var dto = okResult.Value as BookingDto;
        dto!.Status.Should().Be(BookingStatus.Canceled);
    }

    [Fact]
    public async Task Cancel_WithQueuePendingStatus_ShouldReturnOk()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
        var dto = okResult.Value as BookingDto;
        dto!.Status.Should().Be(BookingStatus.Canceled);
    }

    [Fact]
    public async Task Cancel_WithInvalidFormatId_ShouldReturnBadRequest()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var result = await _controller.Cancel("invalid-id", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Cancel_WithException_ShouldReturn500()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
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
    public async Task Delete_WithNonExistentId_ShouldReturnNotFound()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

        var result = await _controller.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldReturnBadRequest()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var result = await _controller.Delete("invalid-id", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_WithException_ShouldReturn500()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        ControllerTestHelper.AssertInternalServerError(result);
    }
}
