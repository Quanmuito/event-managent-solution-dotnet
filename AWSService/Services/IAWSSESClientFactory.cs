namespace AWSService.Services;

using Amazon.SimpleEmail;

public interface IAWSSESClientFactory : IAWSClientFactory<IAmazonSimpleEmailService>
{
}
