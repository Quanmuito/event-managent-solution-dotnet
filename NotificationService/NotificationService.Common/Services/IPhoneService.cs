namespace NotificationService.Common.Services;

public interface IPhoneService
{
    Task SendAsync(string recipient, string content, CancellationToken cancellationToken);
}
