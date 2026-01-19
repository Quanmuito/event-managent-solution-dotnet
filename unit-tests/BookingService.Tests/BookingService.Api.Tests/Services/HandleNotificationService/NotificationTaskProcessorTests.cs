namespace BookingService.Api.Tests.Services.HandleNotificationService;

using BookingService.Api.Messages;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Api.Utils;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using EventService.Data.Models;
using EventService.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NotificationTaskProcessorTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ILogger<NotificationTaskProcessor>> _mockLogger;
    private readonly NotificationTaskProcessor _processor;

    public NotificationTaskProcessorTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockLogger = new Mock<ILogger<NotificationTaskProcessor>>();
        _processor = new NotificationTaskProcessor(_mockEventRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithRegisteredOperation_ShouldProcessSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.Registered);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Processing notification for BookingId: {BookingId}, Operation: {Operation}");
        VerifyLogInformation("Email Subject: {Subject}");
        VerifyLogInformation("Email Content:\n{Content}");
    }

    [Fact]
    public async Task ProcessAsync_WithQueueEnrolledOperation_ShouldProcessSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueueEnrolled);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.QueueEnrolled);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Processing notification for BookingId: {BookingId}, Operation: {Operation}");
        VerifyLogInformation("Email Subject: {Subject}");
        VerifyLogInformation("Email Content:\n{Content}");
    }

    [Fact]
    public async Task ProcessAsync_WithUpdatedOperation_ShouldProcessSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.Updated);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Processing notification for BookingId: {BookingId}, Operation: {Operation}");
    }

    [Fact]
    public async Task ProcessAsync_WithConfirmedOperation_ShouldProcessSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.Confirmed);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Processing notification for BookingId: {BookingId}, Operation: {Operation}");
    }

    [Fact]
    public async Task ProcessAsync_WithCanceledOperation_ShouldProcessSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.Canceled);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Processing notification for BookingId: {BookingId}, Operation: {Operation}");
    }

    [Fact]
    public async Task ProcessAsync_WhenEventNotFound_ShouldLogWarningAndContinue()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.Registered);

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DatabaseService.Exceptions.NotFoundException("Events", booking.EventId));

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogWarning("Failed to fetch event details for EventId: {EventId}");
        VerifyLogInformation("Processing notification for BookingId: {BookingId}, Operation: {Operation}");
    }

    [Fact]
    public async Task ProcessAsync_WithEventHavingDetails_ShouldIncludeDetailsInContent()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.Registered);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");
        eventEntity.Details = "Event details here";

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Email Content:\n{Content}");
    }

    [Fact]
    public async Task ProcessAsync_WithEventWithoutDetails_ShouldNotIncludeDetailsInContent()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new NotificationTaskMessage(bookingDto, BookingOperation.Registered);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");
        eventEntity.Details = null;

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Email Content:\n{Content}");
    }

    private static Event CreateEvent(string id)
    {
        return new Event
        {
            Id = id,
            Title = "Test Event",
            HostedBy = "Test Host",
            IsPublic = true,
            Details = "Test Details",
            TimeStart = DateTime.UtcNow.AddDays(1),
            TimeEnd = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow
        };
    }

    private void VerifyLogInformation(string messageTemplate)
    {
        var prefix = messageTemplate.Split('{')[0];
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(prefix)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogWarning(string messageTemplate)
    {
        var prefix = messageTemplate.Split('{')[0];
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(prefix)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
