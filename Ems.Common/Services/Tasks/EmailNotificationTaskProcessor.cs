namespace Ems.Common.Services.Tasks;

using Ems.Common.Messages;
using NotificationService.Services;
using Microsoft.Extensions.Logging;

public abstract class EmailNotificationTaskProcessor<TData>(
    IEmailService emailService,
    ILogger<EmailNotificationTaskProcessor<TData>> logger
) : ITaskProcessor<EmailNotificationTaskMessage<TData>>
{
    public async Task ProcessAsync(EmailNotificationTaskMessage<TData> message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing email notification for Operation: {Operation}", message.Operation);

        var recipient = GetRecipient(message.Data);
        var subject = GetSubject(message.Operation);
        var content = await ComposeContentAsync(message.Data, message.Operation, cancellationToken);

        await SendEmailAsync(recipient, subject, content, cancellationToken);

        await HandleBusinessLogicAsync(message.Data, message.Operation, cancellationToken);

        logger.LogInformation("Email notification processed successfully");
    }

    protected virtual Task SendEmailAsync(string recipient, string subject, string content, CancellationToken cancellationToken)
    {
        return emailService.SendAsync(recipient, subject, content, cancellationToken);
    }

    protected abstract string GetRecipient(TData data);
    protected abstract Task<string> ComposeContentAsync(TData data, string operation, CancellationToken cancellationToken);
    protected abstract string GetSubject(string operation);
    protected virtual Task HandleBusinessLogicAsync(TData data, string operation, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
