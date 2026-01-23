namespace NotificationService.Tests.Services.Tasks;

using NotificationService.Common.Messages;
using NotificationService.Common.Services;
using NotificationService.Common.Services.Tasks;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;

public class PhoneNotificationTaskProcessorTests
{
    private static (TestPhoneNotificationTaskProcessor processor, Mock<IPhoneService> mockPhoneService, PhoneNotificationTaskMessage<string> message) CreateTestSetup()
    {
        var mockPhoneService = new Mock<IPhoneService>();
        var mockLogger = new Mock<ILogger<TestPhoneNotificationTaskProcessor>>();
        var processor = new TestPhoneNotificationTaskProcessor(mockPhoneService.Object, mockLogger.Object);
        var message = new PhoneNotificationTaskMessage<string>("test-data", "create");
        return (processor, mockPhoneService, message);
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
    public async Task ProcessAsync_ShouldCallSendPhone()
    {
        var (processor, mockPhoneService, message) = CreateTestSetup();

        await processor.ProcessAsync(message, CancellationToken.None);

        mockPhoneService.Verify(x => x.SendAsync(
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
    public async Task ProcessAsync_WhenPhoneServiceThrows_ShouldThrow()
    {
        var (processor, mockPhoneService, message) = CreateTestSetup();
        mockPhoneService.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Phone error"));

        var act = async () => await processor.ProcessAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Phone error");
    }
}

public class TestPhoneNotificationTaskProcessor(IPhoneService phoneService, ILogger<TestPhoneNotificationTaskProcessor> logger) : PhoneNotificationTaskProcessor<string>(phoneService, logger)
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

    protected override Task HandleBusinessLogicAsync(string data, string operation, CancellationToken cancellationToken)
    {
        HandleBusinessLogicCalled = true;
        return Task.CompletedTask;
    }
}
