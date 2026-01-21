namespace Ems.Common.Tests.Extensions.Startup;

using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ems.Common.Extensions.Startup;
using Ems.Common.Services.Tasks;
using Xunit;

public class TaskServiceExtensionsTests
{
    [Fact]
    public void AddTaskService_ShouldRegisterChannel()
    {
        var services = new ServiceCollection();
        services.AddTaskService<string, TestTaskProcessor>();

        var serviceProvider = services.BuildServiceProvider();
        var reader = serviceProvider.GetService<ChannelReader<string>>();
        var writer = serviceProvider.GetService<ChannelWriter<string>>();

        reader.Should().NotBeNull();
        writer.Should().NotBeNull();
    }

    [Fact]
    public void AddTaskService_ShouldRegisterTaskQueue()
    {
        var services = new ServiceCollection();
        services.AddTaskService<string, TestTaskProcessor>();

        var serviceProvider = services.BuildServiceProvider();
        var queue = serviceProvider.GetService<ITaskQueue<string>>();

        queue.Should().NotBeNull();
        queue.Should().BeAssignableTo<ITaskQueue<string>>();
    }

    [Fact]
    public void AddTaskService_ShouldRegisterTaskProcessor()
    {
        var services = new ServiceCollection();
        services.AddTaskService<string, TestTaskProcessor>();

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetService<ITaskProcessor<string>>();

        processor.Should().NotBeNull();
        processor.Should().BeAssignableTo<TestTaskProcessor>();
    }

    [Fact]
    public void AddTaskService_ShouldRegisterHostedService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTaskService<string, TestTaskProcessor>();

        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        hostedServices.Should().Contain(x => x.GetType() == typeof(TaskService<string>));
    }

    [Fact]
    public void AddTaskService_WithCustomCapacity_ShouldUseProvidedCapacity()
    {
        var services = new ServiceCollection();
        var customCapacity = 500;
        services.AddTaskService<string, TestTaskProcessor>(customCapacity);

        var serviceProvider = services.BuildServiceProvider();
        var writer = serviceProvider.GetRequiredService<ChannelWriter<string>>();

        writer.Should().NotBeNull();
    }
}

public class TestTaskProcessor : ITaskProcessor<string>
{
    public Task ProcessAsync(string message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
