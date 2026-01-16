namespace BookingService.Api.Tests.Services;

using Ems.Common.Services.Tasks.Messages;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using Ems.Common.Services.Tasks;
using Moq;
using Xunit;

public class HandleBookingServiceEmailNotificationTests
{
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockTaskQueue;
    private readonly Mock<ITaskQueue<EmailNotificationTaskMessage>> _mockEmailTaskQueue;
    private readonly HandleBookingService _service;

    public HandleBookingServiceEmailNotificationTests()
    {
        _mockRepository = new Mock<IBookingRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        _mockEmailTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage>>();
        _service = new HandleBookingService(_mockRepository.Object, _mockQrCodeRepository.Object, _mockTaskQueue.Object, _mockEmailTaskQueue.Object);
    }

    [Fact]
    public async Task Create_WithRegisteredStatus_ShouldSendEmailNotification()
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

        await _service.Create(dto, CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == dto.Email &&
                m.Subject == "Booking Registered" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithQueueEnrolledStatus_ShouldSendEmailNotification()
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

        await _service.Create(dto, CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == dto.Email &&
                m.Subject == "Queue Enrollment Confirmation" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldSendEmailNotification()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        var updatedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        updatedBooking.Status = updateDto.Status!;
        updatedBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<MongoDB.Driver.UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == updatedBooking.Email &&
                m.Subject == "Booking Update" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNullStatusButOtherFields_ShouldSendEmailNotification()
    {
        var updateDto = new UpdateBookingDto
        {
            Status = null,
            Name = "Updated Name"
        };
        var existingBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        existingBooking.Name = "Updated Name";
        existingBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<MongoDB.Driver.UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == existingBooking.Email &&
                m.Subject == "Booking Update" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldSendEmailNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == booking.Email &&
                m.Subject == "Booking Delete" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithValidId_ShouldSendEmailNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<MongoDB.Driver.UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == canceledBooking.Email &&
                m.Subject == "Booking Cancel" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithQueueEnrolledStatus_ShouldSendEmailNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueueEnrolled);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<MongoDB.Driver.UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == canceledBooking.Email &&
                m.Subject == "Booking Cancel" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithQueuePendingStatus_ShouldSendEmailNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<MongoDB.Driver.UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        _mockEmailTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage>(m =>
                m.RecipientEmail == canceledBooking.Email &&
                m.Subject == "Booking Cancel" &&
                m.ServiceType == "BookingService"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
