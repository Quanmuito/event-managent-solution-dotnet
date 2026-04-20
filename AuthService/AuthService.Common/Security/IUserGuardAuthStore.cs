namespace AuthService.Common.Security;

public interface IUserGuardAuthStore
{
    Task<bool> HasMatchingTokenAsync(string userId, string token, CancellationToken cancellationToken);
}
