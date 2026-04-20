namespace AuthService.Data.Security;

using AuthService.Common.Security;
using AuthService.Data.Repositories;

public class AuthUserGuardAuthStore(IAuthRepository authRepository) : IUserGuardAuthStore
{
    public async Task<bool> HasMatchingTokenAsync(string userId, string token, CancellationToken cancellationToken)
    {
        return await authRepository.HasMatchingTokenAsync(userId, token, cancellationToken);
    }
}
