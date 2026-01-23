namespace NotificationService.Tests.Services;

using AWSService.Settings;
using NotificationService.Common.Services;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Moq;
using TestUtilities.Helpers;
using Xunit;

public class EmailServiceTests
{
    private readonly Mock<IAmazonSimpleEmailService> _mockSesClient;
    private readonly Mock<IOptions<AWSSESSettings>> _mockSettings;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockSesClient = new Mock<IAmazonSimpleEmailService>();
        _mockSettings = new Mock<IOptions<AWSSESSettings>>();
        _mockLogger = new Mock<ILogger<EmailService>>();

        var settings = new AWSSESSettings
        {
            FromEmail = "test@example.com"
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        _emailService = new EmailService(
            _mockSesClient.Object,
            _mockSettings.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendAsync_WithValidRecipient_ShouldSendTextEmail()
    {
        var recipient = "recipient@example.com";
        var subject = "Test Subject";
        var content = "Test Content";
        var messageId = "test-message-id";

        var response = new SendEmailResponse
        {
            MessageId = messageId
        };

        _mockSesClient
            .Setup(x => x.SendEmailAsync(
                It.Is<SendEmailRequest>(r =>
                    r.Source == "test@example.com" &&
                    r.Destination.ToAddresses.Contains(recipient) &&
                    r.Message.Subject.Data == subject &&
                    r.Message.Body.Text.Data == content),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _emailService.SendAsync(recipient, subject, content, CancellationToken.None);

        _mockSesClient.Verify(x => x.SendEmailAsync(
            It.IsAny<SendEmailRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendHtmlAsync_WithValidRecipient_ShouldSendHtmlEmail()
    {
        var recipient = "recipient@example.com";
        var subject = "Test Subject";
        var htmlContent = "<html><body>Test</body></html>";
        var messageId = "test-message-id";

        var response = new SendEmailResponse
        {
            MessageId = messageId
        };

        _mockSesClient
            .Setup(x => x.SendEmailAsync(
                It.Is<SendEmailRequest>(r =>
                    r.Source == "test@example.com" &&
                    r.Destination.ToAddresses.Contains(recipient) &&
                    r.Message.Subject.Data == subject &&
                    r.Message.Body.Html.Data == htmlContent),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _emailService.SendHtmlAsync(recipient, subject, htmlContent, CancellationToken.None);

        _mockSesClient.Verify(x => x.SendEmailAsync(
            It.IsAny<SendEmailRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenSesClientThrows_ShouldLogErrorAndRethrow()
    {
        var recipient = "recipient@example.com";
        var subject = "Test Subject";
        var content = "Test Content";
        var exception = new Exception("SES Error");

        _mockSesClient
            .Setup(x => x.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var act = async () => await _emailService.SendAsync(recipient, subject, content, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("SES Error");

        LoggerTestHelper.VerifyLogError(_mockLogger);
    }

    [Fact]
    public async Task SendAsync_ShouldUseConfiguredFromEmail()
    {
        var recipient = "recipient@example.com";
        var subject = "Test Subject";
        var content = "Test Content";
        var fromEmail = "custom@example.com";

        var settings = new AWSSESSettings
        {
            FromEmail = fromEmail
        };
        _mockSettings.Setup(x => x.Value).Returns(settings);

        var emailService = new EmailService(
            _mockSesClient.Object,
            _mockSettings.Object,
            _mockLogger.Object);

        var response = new SendEmailResponse { MessageId = "test-id" };
        _mockSesClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await emailService.SendAsync(recipient, subject, content, CancellationToken.None);

        _mockSesClient.Verify(x => x.SendEmailAsync(
            It.Is<SendEmailRequest>(r => r.Source == fromEmail),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldLogSuccessfulSend()
    {
        var recipient = "recipient@example.com";
        var subject = "Test Subject";
        var content = "Test Content";
        var messageId = "test-message-id";

        var response = new SendEmailResponse
        {
            MessageId = messageId
        };

        _mockSesClient
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await _emailService.SendAsync(recipient, subject, content, CancellationToken.None);

        LoggerTestHelper.VerifyLogInformation(_mockLogger);
    }

    [Fact]
    public async Task SendAsync_WithNullRecipient_ShouldThrowNullReferenceException()
    {
        var subject = "Test Subject";
        var content = "Test Content";

        var act = async () => await _emailService.SendAsync(null!, subject, content, CancellationToken.None);

        await act.Should().ThrowAsync<NullReferenceException>();
    }
}
