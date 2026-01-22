namespace NotificationService.Services.Tasks;

using Ems.Common.Services.Tasks;
using NotificationService.Messages;
using NotificationService.Services;
using Microsoft.Extensions.Logging;

public abstract class PhoneNotificationTaskProcessor<TData>(
    IPhoneService phoneService,
    ILogger<PhoneNotificationTaskProcessor<TData>> logger
) : ITaskProcessor<PhoneNotificationTaskMessage<TData>>
{
    public async Task ProcessAsync(PhoneNotificationTaskMessage<TData> message, CancellationToken cancellationToken)
    {
        logger.LogInformation("[TASK - PROCESSING] Processing phone notification for Operation: {Operation}", message.Operation);

        var recipient = GetRecipient(message.Data);
        var content = await ComposeContentAsync(message.Data, message.Operation, cancellationToken);

        await SendPhoneAsync(recipient, content, cancellationToken);

        await HandleBusinessLogicAsync(message.Data, message.Operation, cancellationToken);

        logger.LogInformation("[TASK - SUCCESS] Phone notification processed");
    }

    protected virtual Task SendPhoneAsync(string recipient, string content, CancellationToken cancellationToken)
    {
        return phoneService.SendAsync(recipient, content, cancellationToken);
    }

    protected abstract string GetRecipient(TData data);
    protected abstract Task<string> ComposeContentAsync(TData data, string operation, CancellationToken cancellationToken);
    protected virtual Task HandleBusinessLogicAsync(TData data, string operation, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
