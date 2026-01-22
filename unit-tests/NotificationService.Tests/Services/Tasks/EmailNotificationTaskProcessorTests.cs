namespace NotificationService.Tests.Services.Tasks;

using NotificationService.Messages;
using NotificationService.Services;
using NotificationService.Services.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

public class EmailNotificationTaskProcessorTests
{
    private static (TestEmailNotificationTaskProcessor processor, Mock<IEmailService> mockEmailService, EmailNotificationTaskMessage<string> message) CreateTestSetup()
    {
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<TestEmailNotificationTaskProcessor>>();
        var processor = new TestEmailNotificationTaskProcessor(mockEmailService.Object, mockLogger.Object);
        var message = new EmailNotificationTaskMessage<string>("test-data", "create");
        return (processor, mockEmailService, message);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallGetRecipient()
    {
        var (processor, _, message) = CreateTestSetup();

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.GetRecipientCalled.Should().BeTrue();
        processor.LastRecipient.Should().Be("test-data");
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallComposeContent()
    {
        var (processor, _, message) = CreateTestSetup();

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.ComposeContentCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallSendEmail()
    {
        var (processor, mockEmailService, message) = CreateTestSetup();

        await processor.ProcessAsync(message, CancellationToken.None);

        mockEmailService.Verify(x => x.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallHandleBusinessLogic()
    {
        var (processor, _, message) = CreateTestSetup();

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.HandleBusinessLogicCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_WhenEmailServiceThrows_ShouldThrow()
    {
        var (processor, mockEmailService, message) = CreateTestSetup();
        mockEmailService.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email error"));

        var act = async () => await processor.ProcessAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Email error");
    }
}

public class TestEmailNotificationTaskProcessor(IEmailService emailService, ILogger<TestEmailNotificationTaskProcessor> logger) : EmailNotificationTaskProcessor<string>(emailService, logger)
{
    public bool GetRecipientCalled { get; private set; }
    public bool ComposeContentCalled { get; private set; }
    public bool HandleBusinessLogicCalled { get; private set; }
    public string? LastRecipient { get; private set; }

    protected override string GetRecipient(string data)
    {
        GetRecipientCalled = true;
        LastRecipient = data;
        return data;
    }

    protected override Task<string> ComposeContentAsync(string data, string operation, CancellationToken cancellationToken)
    {
        ComposeContentCalled = true;
        return Task.FromResult($"Content for {operation}");
    }

    protected override string GetSubject(string operation)
    {
        return $"Subject for {operation}";
    }

    protected override Task HandleBusinessLogicAsync(string data, string operation, CancellationToken cancellationToken)
    {
        HandleBusinessLogicCalled = true;
        return Task.CompletedTask;
    }
}
