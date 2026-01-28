namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Models;
using BookingService.Api.Utils;
using BookingService.Data.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using EventService.Data.Models;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class BookingControllerNotificationTests : IClassFixture<BookingControllerTestFixture>
{
    private readonly BookingControllerTestFixture _fixture;

    public BookingControllerNotificationTests(BookingControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task Create_WithRegisteredStatus_ShouldTriggerNotification()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        createDto.Status = BookingStatus.Registered;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = BookingStatus.Registered;
        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = createDto.EventId });
        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Registered);
    }

    [Fact]
    public async Task Create_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        createDto.Status = BookingStatus.QueueEnrolled;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = BookingStatus.QueueEnrolled;
        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = createDto.EventId });
        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _fixture.Controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.QueueEnrolled);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldTriggerNotification()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        var updatedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        updatedBooking.Name = updateDto.Name!;
        updatedBooking.UpdatedAt = DateTime.UtcNow;
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        var result = await _fixture.Controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Updated);
    }

    [Fact]
    public async Task Confirm_WithValidId_ShouldTriggerNotification()
    {
        var confirmedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        confirmedBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.ConfirmAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmedBooking);

        var result = await _fixture.Controller.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
        var dto = okResult.Value as BookingDto;
        dto!.Status.Should().Be(BookingStatus.Registered);
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Confirmed);
    }

    [Fact]
    public async Task Cancel_WithValidId_ShouldTriggerNotification()
    {
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Canceled);
    }

    [Fact]
    public async Task Cancel_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Canceled);
    }

    [Fact]
    public async Task Cancel_WithQueuePendingStatus_ShouldTriggerNotification()
    {
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Canceled);
    }
}
