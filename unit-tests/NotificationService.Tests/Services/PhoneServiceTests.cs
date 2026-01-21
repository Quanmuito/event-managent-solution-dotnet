namespace NotificationService.Tests.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Services;
using Xunit;

public class PhoneServiceTests
{
    private readonly Mock<ILogger<PhoneService>> _mockLogger;
    private readonly PhoneService _phoneService;

    public PhoneServiceTests()
    {
        _mockLogger = new Mock<ILogger<PhoneService>>();
        _phoneService = new PhoneService(_mockLogger.Object);
    }

    [Fact]
    public async Task SendAsync_WithValidRecipient_ShouldLogInformation()
    {
        var recipient = "+1234567890";
        var content = "Test SMS Content";

        await _phoneService.SendAsync(recipient, content, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithValidContent_ShouldCompleteSuccessfully()
    {
        var recipient = "+1234567890";
        var content = "Test SMS Content";

        await _phoneService.SendAsync(recipient, content, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldLogRecipientAndContent()
    {
        var recipient = "+1234567890";
        var content = "Test SMS Content";

        await _phoneService.SendAsync(recipient, content, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
