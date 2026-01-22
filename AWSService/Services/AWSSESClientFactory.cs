namespace AWSService.Services;

using AWSService.Settings;
using Amazon.SimpleEmail;
using Microsoft.Extensions.Options;

public class AWSSESClientFactory(IOptions<AWSSESSettings> settings)
    : AWSClientFactoryBase<IAmazonSimpleEmailService, AmazonSimpleEmailServiceConfig, AWSSESSettings>(settings),
      IAWSSESClientFactory
{
    protected override IAmazonSimpleEmailService CreateClientCore(
        AmazonSimpleEmailServiceConfig config,
        string? accessKey,
        string? secretKey)
    {
        return !string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey)
            ? new AmazonSimpleEmailServiceClient(accessKey, secretKey, config)
            : new AmazonSimpleEmailServiceClient(config);
    }
}
