namespace Ems.Common.Tests.Services.Tasks;

using System.Threading.Channels;
using FluentAssertions;
using Ems.Common.Services.Tasks;
using Xunit;

public class TaskQueueTests
{
    [Fact]
    public async Task EnqueueAsync_WithValidMessage_ShouldWriteToChannel()
    {
        var channel = Channel.CreateUnbounded<string>();
        var queue = new TaskQueue<string>(channel.Writer);
        var message = "test-message";

        await queue.EnqueueAsync(message, CancellationToken.None);

        var result = await channel.Reader.ReadAsync();
        result.Should().Be(message);
    }

    [Fact]
    public async Task EnqueueAsync_WhenChannelFull_ShouldWaitOrBlock()
    {
        var options = new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        var channel = Channel.CreateBounded<string>(options);
        var queue = new TaskQueue<string>(channel.Writer);

        await queue.EnqueueAsync("message1", CancellationToken.None);

        var enqueueTask = queue.EnqueueAsync("message2", CancellationToken.None);
        enqueueTask.IsCompleted.Should().BeFalse();

        await channel.Reader.ReadAsync();
        await enqueueTask;
        enqueueTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task EnqueueAsync_WithCancellation_ShouldCancelOperation()
    {
        var channel = Channel.CreateUnbounded<string>();
        var queue = new TaskQueue<string>(channel.Writer);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await queue.EnqueueAsync("message", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EnqueueAsync_MultipleMessages_ShouldEnqueueInOrder()
    {
        var channel = Channel.CreateUnbounded<string>();
        var queue = new TaskQueue<string>(channel.Writer);

        await queue.EnqueueAsync("message1", CancellationToken.None);
        await queue.EnqueueAsync("message2", CancellationToken.None);
        await queue.EnqueueAsync("message3", CancellationToken.None);

        var message1 = await channel.Reader.ReadAsync();
        var message2 = await channel.Reader.ReadAsync();
        var message3 = await channel.Reader.ReadAsync();

        message1.Should().Be("message1");
        message2.Should().Be("message2");
        message3.Should().Be("message3");
    }
}
