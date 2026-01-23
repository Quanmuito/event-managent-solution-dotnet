namespace NotificationService.Common.Messages;

public record PhoneNotificationTaskMessage<TData>(
    TData Data,
    string Operation
);
