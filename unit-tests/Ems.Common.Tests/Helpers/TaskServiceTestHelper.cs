namespace Ems.Common.Tests.Helpers;

using System.Threading.Channels;

public static class TaskServiceTestHelper
{
    public static Channel<T> CreateUnboundedChannel<T>()
    {
        return Channel.CreateUnbounded<T>();
    }

    public static Channel<T> CreateBoundedChannel<T>(int capacity, BoundedChannelFullMode fullMode = BoundedChannelFullMode.Wait)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = fullMode
        };
        return Channel.CreateBounded<T>(options);
    }
}
