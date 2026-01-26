namespace QueueService.Tests.Helpers;

using QueueService.Data.Models;

public static class TestDataBuilder
{
    public static Queue CreateQueue(string? id = null, string? eventId = null, int available = 0, int top = 0)
    {
        return new Queue
        {
            Id = id ?? "507f1f77bcf86cd799439011",
            EventId = eventId ?? "507f1f77bcf86cd799439012",
            Available = available,
            Top = top,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    public static List<Queue> CreateQueueList(int count = 3)
    {
        var queues = new List<Queue>();
        for (int i = 0; i < count; i++)
        {
            queues.Add(CreateQueue($"507f1f77bcf86cd79943901{i}", $"507f1f77bcf86cd79943902{i}", i, i));
        }
        return queues;
    }
}
