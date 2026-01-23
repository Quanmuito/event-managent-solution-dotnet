namespace NotificationService.Common.Messages;

public record EmailNotificationTaskMessage<TData>(
    TData Data,
    string Operation
);
