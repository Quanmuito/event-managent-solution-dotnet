namespace Ems.Common.Services.Tasks.Messages;

public record EmailNotificationTaskMessage(
    string RecipientEmail,
    string Subject,
    string Body,
    string? ServiceType = null,
    Dictionary<string, object>? Metadata = null
);
