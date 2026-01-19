namespace Ems.Common.Messages;

public record PhoneNotificationTaskMessage<TData>(
    TData Data,
    string Operation
);
