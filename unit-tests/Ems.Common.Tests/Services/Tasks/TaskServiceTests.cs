namespace Ems.Common.Tests.Services.Tasks;

using Ems.Common.Services.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ems.Common.Tests.Helpers;
using FluentAssertions;
using Moq;
using System.Threading.Channels;
using Xunit;

public class TaskServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithMessage_ShouldProcessViaProcessor()
    {
        var channel = TaskServiceTestHelper.CreateUnboundedChannel<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        var (service, _) = CreateTaskService(channel, mockProcessor.Object);

        await channel.Writer.WriteAsync("test-message");
        channel.Writer.Complete();
        await RunServiceWithDelay(service, TimeSpan.FromSeconds(1), 100);

        mockProcessor.Verify(x => x.ProcessAsync("test-message", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProcessorThrows_ShouldLogErrorAndContinue()
    {
        var channel = TaskServiceTestHelper.CreateUnboundedChannel<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        mockProcessor.Setup(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));
        var (service, mockLogger) = CreateTaskService(channel, mockProcessor.Object);

        await channel.Writer.WriteAsync("test-message");
        channel.Writer.Complete();
        await RunServiceWithDelay(service, TimeSpan.FromSeconds(1), 100);

        LoggerTestHelper.VerifyErrorLogged(mockLogger);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMessages_ShouldProcessAll()
    {
        var channel = TaskServiceTestHelper.CreateUnboundedChannel<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        var (service, _) = CreateTaskService(channel, mockProcessor.Object);

        await channel.Writer.WriteAsync("message1");
        await channel.Writer.WriteAsync("message2");
        channel.Writer.Complete();
        await RunServiceWithDelay(service, TimeSpan.FromSeconds(1), 100);

        mockProcessor.Verify(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldStopProcessing()
    {
        var channel = TaskServiceTestHelper.CreateUnboundedChannel<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        var (service, _) = CreateTaskService(channel, mockProcessor.Object);
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200);
        await service.StopAsync(CancellationToken.None);

        cancellationTokenSource.Token.IsCancellationRequested.Should().BeTrue();
    }

    private static (TaskService<string> service, Mock<ILogger<TaskService<string>>> logger) CreateTaskService(
        Channel<string> channel, ITaskProcessor<string> processor)
    {
        var mockLogger = new Mock<ILogger<TaskService<string>>>();
        var serviceProvider = CreateServiceProvider(processor);
        var service = new TaskService<string>(channel.Reader, serviceProvider, mockLogger.Object);

        return (service, mockLogger);
    }

    private static async Task RunServiceWithDelay(TaskService<string> service, TimeSpan timeout, int delayMs)
    {
        var cancellationTokenSource = new CancellationTokenSource(timeout);
        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(delayMs);
        await service.StopAsync(cancellationTokenSource.Token);
    }

    private static IServiceProvider CreateServiceProvider(ITaskProcessor<string> processor)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => processor);
        return services.BuildServiceProvider();
    }
}
