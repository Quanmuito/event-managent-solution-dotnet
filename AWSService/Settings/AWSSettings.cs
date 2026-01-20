namespace AWSService.Settings;

using System.ComponentModel.DataAnnotations;

public abstract class AWSSettings
{
    [Required]
    public string Region { get; set; } = "us-east-1";

    public string? ServiceURL { get; set; }

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;
}
