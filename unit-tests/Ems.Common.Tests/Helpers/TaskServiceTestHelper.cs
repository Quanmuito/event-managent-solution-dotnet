namespace Ems.Common.Tests.Helpers;

using System.Threading.Channels;
using Ems.Common.Services.Tasks;

public static class TaskServiceTestHelper
{
    public static (ChannelReader<T>, ChannelWriter<T>) CreateTestChannel<T>(int capacity = 1000)
    {
        var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
        return (channel.Reader, channel.Writer);
    }
}
