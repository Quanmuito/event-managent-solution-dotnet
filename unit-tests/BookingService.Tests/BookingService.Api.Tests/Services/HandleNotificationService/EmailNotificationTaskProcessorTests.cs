namespace BookingService.Api.Tests.Services.HandleNotificationService;

using BookingService.Api.Services;
using Ems.Common.Services.Tasks.Messages;
using BookingService.Api.Messages;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class EmailNotificationTaskProcessorTests
{
    private readonly Mock<ILogger<EmailNotificationTaskProcessor>> _mockLogger;
    private readonly EmailNotificationTaskProcessor _processor;

    public EmailNotificationTaskProcessorTests()
    {
        _mockLogger = new Mock<ILogger<EmailNotificationTaskProcessor>>();
        _processor = new EmailNotificationTaskProcessor(_mockLogger.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithValidMessage_ShouldLogEmailDetails()
    {
        var message = new EmailNotificationTaskMessage(
            RecipientEmail: "test@example.com",
            Subject: "Test Subject",
            Body: "Test Body",
            ServiceType: "BookingService",
            Metadata: new Dictionary<string, object> { { "BookingId", "507f1f77bcf86cd799439011" } }
        );

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("test@example.com") && v.ToString()!.Contains("Test Subject")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessAsync_WithBookingIdInMetadata_ShouldLogBookingId()
    {
        var message = new EmailNotificationTaskMessage(
            RecipientEmail: "test@example.com",
            Subject: "Test Subject",
            Body: "Test Body",
            ServiceType: "BookingService",
            Metadata: new Dictionary<string, object> { { "BookingId", "507f1f77bcf86cd799439011" } }
        );

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("507f1f77bcf86cd799439011")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithoutMetadata_ShouldNotThrow()
    {
        var message = new EmailNotificationTaskMessage(
            RecipientEmail: "test@example.com",
            Subject: "Test Subject",
            Body: "Test Body",
            ServiceType: "BookingService",
            Metadata: null
        );

        var act = async () => await _processor.ProcessAsync(message, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyMetadata_ShouldNotThrow()
    {
        var message = new EmailNotificationTaskMessage(
            RecipientEmail: "test@example.com",
            Subject: "Test Subject",
            Body: "Test Body",
            ServiceType: "BookingService",
            Metadata: []
        );

        var act = async () => await _processor.ProcessAsync(message, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessAsync_WithMetadataButNoBookingId_ShouldNotLogBookingId()
    {
        var message = new EmailNotificationTaskMessage(
            RecipientEmail: "test@example.com",
            Subject: "Test Subject",
            Body: "Test Body",
            ServiceType: "BookingService",
            Metadata: new Dictionary<string, object> { { "OtherKey", "OtherValue" } }
        );

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing notification for Booking ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
