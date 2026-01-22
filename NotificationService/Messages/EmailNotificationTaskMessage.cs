namespace NotificationService.Messages;

public record EmailNotificationTaskMessage<TData>(
    TData Data,
    string Operation
);
