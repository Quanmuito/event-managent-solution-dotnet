namespace AWSService.Services;

public interface IAWSClientFactory<out TClient>
{
    TClient CreateClient();
}
