namespace Ems.Common.Tests.Services.Tasks;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Ems.Common.Messages;
using Ems.Common.Services.Tasks;
using NotificationService.Services;
using Xunit;

public class PhoneNotificationTaskProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldCallGetRecipient()
    {
        var mockPhoneService = new Mock<IPhoneService>();
        var mockLogger = new Mock<ILogger<TestPhoneNotificationTaskProcessor>>();
        var processor = new TestPhoneNotificationTaskProcessor(mockPhoneService.Object, mockLogger.Object);
        var message = new PhoneNotificationTaskMessage<string>("test-data", "create");

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.GetRecipientCalled.Should().BeTrue();
        processor.LastRecipient.Should().Be("test-data");
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallComposeContent()
    {
        var mockPhoneService = new Mock<IPhoneService>();
        var mockLogger = new Mock<ILogger<TestPhoneNotificationTaskProcessor>>();
        var processor = new TestPhoneNotificationTaskProcessor(mockPhoneService.Object, mockLogger.Object);
        var message = new PhoneNotificationTaskMessage<string>("test-data", "create");

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.ComposeContentCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallSendPhone()
    {
        var mockPhoneService = new Mock<IPhoneService>();
        var mockLogger = new Mock<ILogger<TestPhoneNotificationTaskProcessor>>();
        var processor = new TestPhoneNotificationTaskProcessor(mockPhoneService.Object, mockLogger.Object);
        var message = new PhoneNotificationTaskMessage<string>("test-data", "create");

        await processor.ProcessAsync(message, CancellationToken.None);

        mockPhoneService.Verify(x => x.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCallHandleBusinessLogic()
    {
        var mockPhoneService = new Mock<IPhoneService>();
        var mockLogger = new Mock<ILogger<TestPhoneNotificationTaskProcessor>>();
        var processor = new TestPhoneNotificationTaskProcessor(mockPhoneService.Object, mockLogger.Object);
        var message = new PhoneNotificationTaskMessage<string>("test-data", "create");

        await processor.ProcessAsync(message, CancellationToken.None);

        processor.HandleBusinessLogicCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAsync_WhenPhoneServiceThrows_ShouldThrow()
    {
        var mockPhoneService = new Mock<IPhoneService>();
        mockPhoneService.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Phone error"));
        var mockLogger = new Mock<ILogger<TestPhoneNotificationTaskProcessor>>();
        var processor = new TestPhoneNotificationTaskProcessor(mockPhoneService.Object, mockLogger.Object);
        var message = new PhoneNotificationTaskMessage<string>("test-data", "create");

        var act = async () => await processor.ProcessAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Phone error");
    }
}

public class TestPhoneNotificationTaskProcessor : PhoneNotificationTaskProcessor<string>
{
    public bool GetRecipientCalled { get; private set; }
    public bool ComposeContentCalled { get; private set; }
    public bool HandleBusinessLogicCalled { get; private set; }
    public string? LastRecipient { get; private set; }

    public TestPhoneNotificationTaskProcessor(IPhoneService phoneService, ILogger<TestPhoneNotificationTaskProcessor> logger)
        : base(phoneService, logger)
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

    protected override Task HandleBusinessLogicAsync(string data, string operation, CancellationToken cancellationToken)
    {
        HandleBusinessLogicCalled = true;
        return Task.CompletedTask;
    }
}
