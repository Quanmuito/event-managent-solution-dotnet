namespace Ems.Common.Messages;

public record EmailNotificationTaskMessage<TData>(
    TData Data,
    string Operation
);
