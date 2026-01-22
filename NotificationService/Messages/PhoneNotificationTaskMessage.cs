namespace NotificationService.Messages;

public record PhoneNotificationTaskMessage<TData>(
    TData Data,
    string Operation
);
