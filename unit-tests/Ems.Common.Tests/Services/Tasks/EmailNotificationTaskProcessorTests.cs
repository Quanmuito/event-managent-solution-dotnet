namespace Ems.Common.Tests.Services.Tasks;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Ems.Common.Messages;
using Ems.Common.Services.Tasks;
using NotificationService.Services;
using Xunit;

public class EmailNotificationTaskProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldCallGetRecipient()
    {
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<TestEmailNotificationTaskProcessor>>();
        var processor = new TestEmailNotificationTaskProcessor(mockEmailService.Object, mockLogger.Object);
        var message = new EmailNotificationTaskMessage<string>("test-data", "create");

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.GetRecipientCalled.Should().BeTrue();
        processor.LastRecipient.Should().Be("test-data");
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallComposeContent()
    {
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<TestEmailNotificationTaskProcessor>>();
        var processor = new TestEmailNotificationTaskProcessor(mockEmailService.Object, mockLogger.Object);
        var message = new EmailNotificationTaskMessage<string>("test-data", "create");

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.ComposeContentCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallSendEmail()
    {
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<TestEmailNotificationTaskProcessor>>();
        var processor = new TestEmailNotificationTaskProcessor(mockEmailService.Object, mockLogger.Object);
        var message = new EmailNotificationTaskMessage<string>("test-data", "create");

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
        var mockEmailService = new Mock<IEmailService>();
        var mockLogger = new Mock<ILogger<TestEmailNotificationTaskProcessor>>();
        var processor = new TestEmailNotificationTaskProcessor(mockEmailService.Object, mockLogger.Object);
        var message = new EmailNotificationTaskMessage<string>("test-data", "create");

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.HandleBusinessLogicCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_WhenEmailServiceThrows_ShouldThrow()
    {
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email error"));
        var mockLogger = new Mock<ILogger<TestEmailNotificationTaskProcessor>>();
        var processor = new TestEmailNotificationTaskProcessor(mockEmailService.Object, mockLogger.Object);
        var message = new EmailNotificationTaskMessage<string>("test-data", "create");

        var act = async () => await processor.ProcessAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Email error");
    }
}

public class TestEmailNotificationTaskProcessor : EmailNotificationTaskProcessor<string>
{
    public bool GetRecipientCalled { get; private set; }
    public bool ComposeContentCalled { get; private set; }
    public bool HandleBusinessLogicCalled { get; private set; }
    public string? LastRecipient { get; private set; }

    public TestEmailNotificationTaskProcessor(IEmailService emailService, ILogger<TestEmailNotificationTaskProcessor> logger)
        : base(emailService, logger)
    {
    }

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
