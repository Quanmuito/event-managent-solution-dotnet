namespace BookingService.Api.Tests.Services.HandleBookingService;

using BookingService.Api.Models;
using BookingService.Api.Utils;
using BookingService.Data.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using EventService.Data.Models;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleBookingServiceNotificationTests : IClassFixture<HandleBookingServiceTestFixture>
{
    private readonly HandleBookingServiceTestFixture _fixture;

    public HandleBookingServiceNotificationTests(HandleBookingServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task Create_WithRegisteredStatus_ShouldTriggerNotification()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        dto.Status = BookingStatus.Registered;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = dto.Status;

        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(dto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = dto.EventId });
        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Registered);
    }

    [Fact]
    public async Task Create_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        dto.Status = BookingStatus.QueueEnrolled;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = dto.Status;

        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(dto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = dto.EventId });
        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
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

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Updated);
    }

    [Fact]
    public async Task Update_WithNameField_ShouldTriggerNotification()
    {
        var updateDto = new UpdateBookingDto
        {
            Name = "Updated Name"
        };
        var existingBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        existingBooking.Name = "Updated Name";
        existingBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Updated);
    }

    [Fact]
    public async Task Cancel_WithValidId_ShouldTriggerNotification()
    {
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Canceled);
    }

    [Fact]
    public async Task Cancel_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Canceled);
    }

    [Fact]
    public async Task Cancel_WithQueuePendingStatus_ShouldTriggerNotification()
    {
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Canceled);
    }

    [Fact]
    public async Task Confirm_WithValidId_ShouldTriggerNotification()
    {
        var confirmedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        confirmedBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.ConfirmAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmedBooking);

        var result = await _fixture.Service.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Registered);
        _fixture.VerifyNotifications("507f1f77bcf86cd799439011", BookingOperation.Confirmed);
    }
}
