namespace BookingService.Api.Tests.Services;

using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Api.Messages;
using BookingService.Api.Utils;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using Ems.Common.Services.Tasks;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleBookingServiceNotificationTests
{
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockQrCodeTaskQueue;
    private readonly Mock<ITaskQueue<NotificationTaskMessage>> _mockNotificationTaskQueue;
    private readonly HandleBookingService _service;

    public HandleBookingServiceNotificationTests()
    {
        _mockRepository = new Mock<IBookingRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockQrCodeTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        _mockNotificationTaskQueue = new Mock<ITaskQueue<NotificationTaskMessage>>();
        _service = new HandleBookingService(_mockRepository.Object, _mockQrCodeRepository.Object, _mockQrCodeTaskQueue.Object, _mockNotificationTaskQueue.Object);
    }

    [Fact]
    public async Task Create_WithRegisteredStatus_ShouldTriggerNotification()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        dto.Status = BookingStatus.Registered;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = dto.Status;

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Registered),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        dto.Status = BookingStatus.QueueEnrolled;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = dto.Status;

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.QueueEnrolled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldTriggerNotification()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        var updatedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        updatedBooking.Status = updateDto.Status!;
        updatedBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        var result = await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Updated),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNullStatusButOtherFields_ShouldTriggerNotification()
    {
        var updateDto = new UpdateBookingDto
        {
            Status = null,
            Name = "Updated Name"
        };
        var existingBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        existingBooking.Name = "Updated Name";
        existingBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        var result = await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Updated),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithValidId_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueueEnrolled);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithQueuePendingStatus_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Confirm_WithValidId_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var confirmedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        confirmedBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmedBooking);

        var result = await _service.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Registered);
        _mockNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<NotificationTaskMessage>(m => m.Booking.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Confirmed),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
