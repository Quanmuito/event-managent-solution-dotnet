namespace AWSService.Settings;

using System.ComponentModel.DataAnnotations;

public class AWSSESSettings : AWSSettings
{
    [Required]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;
}
