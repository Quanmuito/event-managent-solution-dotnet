namespace BookingService.Api.Tests.Services.HandleNotificationService;

using System.Collections.Generic;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Api.Utils;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using Ems.Common.Messages;
using Ems.Common.Services.Notification;
using EventService.Data.Models;
using EventService.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BookingPhoneNotificationTaskProcessorTests
{
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<IPhoneService> _mockPhoneService;
    private readonly Mock<ILogger<BookingPhoneNotificationTaskProcessor>> _mockLogger;
    private readonly BookingPhoneNotificationTaskProcessor _processor;

    public BookingPhoneNotificationTaskProcessorTests()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockPhoneService = new Mock<IPhoneService>();
        _mockLogger = new Mock<ILogger<BookingPhoneNotificationTaskProcessor>>();
        _processor = new BookingPhoneNotificationTaskProcessor(
            _mockPhoneService.Object,
            _mockEventRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithRegisteredOperation_ShouldProcessSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new PhoneNotificationTaskMessage<BookingDto>(bookingDto, BookingOperation.Registered);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneService.Verify(x => x.SendAsync(booking.Phone, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Processing phone notification for Operation: {Operation}");
    }

    [Fact]
    public async Task ProcessAsync_WithQueueEnrolledOperation_ShouldProcessSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueueEnrolled);
        var bookingDto = new BookingDto(booking);
        var message = new PhoneNotificationTaskMessage<BookingDto>(bookingDto, BookingOperation.QueueEnrolled);
        var eventEntity = CreateEvent("507f1f77bcf86cd799439012");

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(eventEntity);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneService.Verify(x => x.SendAsync(booking.Phone, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogInformation("Processing phone notification for Operation: {Operation}");
    }

    [Fact]
    public async Task ProcessAsync_WhenEventNotFound_ShouldLogWarningAndContinue()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var bookingDto = new BookingDto(booking);
        var message = new PhoneNotificationTaskMessage<BookingDto>(bookingDto, BookingOperation.Registered);

        _mockEventRepository.Setup(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Events with ID '{booking.EventId}' was not found."));

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockEventRepository.Verify(x => x.GetByIdAsync(booking.EventId, It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneService.Verify(x => x.SendAsync(booking.Phone, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogWarning("Failed to fetch event details for EventId: {EventId}");
        VerifyLogInformation("Processing phone notification for Operation: {Operation}");
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
