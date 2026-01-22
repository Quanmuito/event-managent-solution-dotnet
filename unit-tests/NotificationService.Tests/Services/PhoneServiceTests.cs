namespace NotificationService.Tests.Services;

using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Services;
using TestUtilities.Helpers;
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
    public async Task SendAsync_WithValidInput_ShouldLogInformationAndDebug()
    {
        var recipient = "+1234567890";
        var content = "Test SMS Content";

        await _phoneService.SendAsync(recipient, content, CancellationToken.None);

        LoggerTestHelper.VerifyLogInformation(_mockLogger);
        LoggerTestHelper.VerifyLogDebug(_mockLogger);
    }
}
