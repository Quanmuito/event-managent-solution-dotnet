namespace NotificationService.Common.Services;

public interface IEmailService
{
    Task SendAsync(string recipient, string subject, string content, CancellationToken cancellationToken);
    Task SendHtmlAsync(string recipient, string subject, string htmlContent, CancellationToken cancellationToken);
}
