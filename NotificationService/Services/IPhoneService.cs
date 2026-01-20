namespace NotificationService.Services;

public interface IPhoneService
{
    Task SendAsync(string recipient, string content, CancellationToken cancellationToken);
}
