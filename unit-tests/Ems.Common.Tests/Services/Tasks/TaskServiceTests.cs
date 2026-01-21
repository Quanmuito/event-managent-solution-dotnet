namespace Ems.Common.Tests.Services.Tasks;

using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Ems.Common.Services.Tasks;
using Xunit;

public class TaskServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithMessage_ShouldProcessViaProcessor()
    {
        var channel = Channel.CreateUnbounded<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        var mockLogger = new Mock<ILogger<TaskService<string>>>();
        var serviceProvider = CreateServiceProvider(mockProcessor.Object);

        var service = new TaskService<string>(channel.Reader, serviceProvider, mockLogger.Object);
        await channel.Writer.WriteAsync("test-message");
        channel.Writer.Complete();

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(cancellationTokenSource.Token);

        mockProcessor.Verify(x => x.ProcessAsync("test-message", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProcessorThrows_ShouldLogErrorAndContinue()
    {
        var channel = Channel.CreateUnbounded<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        mockProcessor.Setup(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));
        var mockLogger = new Mock<ILogger<TaskService<string>>>();
        var serviceProvider = CreateServiceProvider(mockProcessor.Object);

        var service = new TaskService<string>(channel.Reader, serviceProvider, mockLogger.Object);
        await channel.Writer.WriteAsync("test-message");
        channel.Writer.Complete();

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(cancellationTokenSource.Token);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMessages_ShouldProcessAll()
    {
        var channel = Channel.CreateUnbounded<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        var mockLogger = new Mock<ILogger<TaskService<string>>>();
        var serviceProvider = CreateServiceProvider(mockProcessor.Object);

        var service = new TaskService<string>(channel.Reader, serviceProvider, mockLogger.Object);
        await channel.Writer.WriteAsync("message1");
        await channel.Writer.WriteAsync("message2");
        channel.Writer.Complete();

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(cancellationTokenSource.Token);

        mockProcessor.Verify(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateScopePerMessage()
    {
        var channel = Channel.CreateUnbounded<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        var mockLogger = new Mock<ILogger<TaskService<string>>>();
        var serviceProvider = CreateServiceProvider(mockProcessor.Object);

        var service = new TaskService<string>(channel.Reader, serviceProvider, mockLogger.Object);
        await channel.Writer.WriteAsync("message1");
        await channel.Writer.WriteAsync("message2");
        channel.Writer.Complete();

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(cancellationTokenSource.Token);

        mockProcessor.Verify(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldStopProcessing()
    {
        var channel = Channel.CreateUnbounded<string>();
        var mockProcessor = new Mock<ITaskProcessor<string>>();
        var mockLogger = new Mock<ILogger<TaskService<string>>>();
        var serviceProvider = CreateServiceProvider(mockProcessor.Object);

        var service = new TaskService<string>(channel.Reader, serviceProvider, mockLogger.Object);
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(200);
        await service.StopAsync(CancellationToken.None);

        cancellationTokenSource.Token.IsCancellationRequested.Should().BeTrue();
    }

    private static IServiceProvider CreateServiceProvider(ITaskProcessor<string> processor)
    {
        var services = new ServiceCollection();
        services.AddScoped<ITaskProcessor<string>>(_ => processor);
        return services.BuildServiceProvider();
    }
}
