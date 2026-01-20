namespace NotificationService.Services;

using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AWSService.Settings;

public class EmailService(
    IAmazonSimpleEmailService sesClient,
    IOptions<AWSSESSettings> settings,
    ILogger<EmailService> logger
) : IEmailService
{
    private readonly AWSSESSettings _settings = settings.Value;

    public Task SendAsync(string recipient, string subject, string content, CancellationToken cancellationToken)
    {
        return SendEmailAsync(recipient, subject, content, null, cancellationToken);
    }

    public Task SendHtmlAsync(string recipient, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        return SendEmailAsync(recipient, subject, null, htmlContent, cancellationToken);
    }

    private async Task SendEmailAsync(
        string recipient,
        string subject,
        string? textContent,
        string? htmlContent,
        CancellationToken cancellationToken)
    {
        var body = new Body();

        if (!string.IsNullOrEmpty(textContent))
        {
            body.Text = new Content(textContent);
        }

        if (!string.IsNullOrEmpty(htmlContent))
        {
            body.Html = new Content(htmlContent);
        }

        var request = new SendEmailRequest
        {
            Source = _settings.FromEmail,
            Destination = new Destination
            {
                ToAddresses = [recipient]
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = body
            }
        };

        try
        {
            var response = await sesClient.SendEmailAsync(request, cancellationToken);
            logger.LogInformation(
                "Email sent to {Recipient} with subject: {Subject}, MessageId: {MessageId}",
                recipient, subject, response.MessageId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to send email to {Recipient} with subject: {Subject}",
                recipient, subject);
            throw;
        }
    }
}
