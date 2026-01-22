namespace AWSService.Services;

using AWSService.Settings;
using Amazon;
using Amazon.Runtime;
using Microsoft.Extensions.Options;

public abstract class AWSClientFactoryBase<TClient, TConfig, TSettings>(IOptions<TSettings> settings)
    where TClient : class
    where TConfig : ClientConfig, new()
    where TSettings : AWSSettings
{
    protected readonly TSettings Settings = settings.Value;

    public TClient CreateClient()
    {
        var config = new TConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(Settings.Region)
        };

        if (!string.IsNullOrEmpty(Settings.ServiceURL))
            config.ServiceURL = Settings.ServiceURL;

        return !string.IsNullOrEmpty(Settings.AccessKey) && !string.IsNullOrEmpty(Settings.SecretKey)
            ? CreateClientCore(config, Settings.AccessKey, Settings.SecretKey)
            : CreateClientCore(config, null, null);
    }

    protected abstract TClient CreateClientCore(TConfig config, string? accessKey, string? secretKey);
}
