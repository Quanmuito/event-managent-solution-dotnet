namespace AuthService.Api.Services;

using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Data.Repositories;
using MongoDB.Driver;
using UserService.Data.Models;
using UserService.Data.Repositories;

public class HandleAuthService(IAuthRepository authRepository, IUserRepository userRepository, IJwtTokenService jwtTokenService)
{
    public async Task<RegisterResultDto> Register(RegisterDto registerDto, CancellationToken cancellationToken)
    {
        var newUser = new User
        {
            Email = registerDto.Email,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };
        var createdUser = await userRepository.CreateAsync(newUser, cancellationToken);
        if (string.IsNullOrWhiteSpace(createdUser.Id))
            throw new InvalidOperationException("User id was not generated.");

        var token = jwtTokenService.GenerateToken(createdUser.Id!, createdUser.Email, createdUser.Roles);

        var newAuth = new Auth
        {
            UserId = createdUser.Id!,
            PasswordHash = registerDto.PasswordHash,
            Token = token,
            CreatedAt = DateTime.UtcNow
        };
        await authRepository.CreateAsync(newAuth, cancellationToken);

        return new RegisterResultDto
        {
            Message = "Register success.",
            Token = token
        };
    }

    public async Task<string> Login(LoginDto loginDto, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(loginDto.Email, cancellationToken);
        if (user == null || string.IsNullOrWhiteSpace(user.Id))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var auth = await authRepository.GetByUserIdOrThrowAsync(user.Id, cancellationToken);
        if (string.IsNullOrWhiteSpace(auth.Id))
            throw new InvalidOperationException("Auth id was not found.");

        if (auth.PasswordHash != loginDto.PasswordHash)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var token = jwtTokenService.GenerateToken(user.Id, user.Email, user.Roles);
        var updates = Builders<Auth>.Update.Combine(
            Builders<Auth>.Update.Set(a => a.Token, token),
            Builders<Auth>.Update.Set(a => a.UpdatedAt, DateTime.UtcNow)
        );
        await authRepository.UpdateAsync(auth.Id, updates, cancellationToken);

        return token;
    }

    public async Task<AuthDto> GetById(string id, CancellationToken cancellationToken)
    {
        var auth = await authRepository.GetByIdAsync(id, cancellationToken);
        return new AuthDto(auth);
    }

    public async Task<Auth> Create(CreateAuthDto createDto, CancellationToken cancellationToken)
    {
        var newAuth = new Auth
        {
            UserId = createDto.UserId,
            PasswordHash = createDto.PasswordHash,
            Token = createDto.Token,
            CreatedAt = DateTime.UtcNow
        };
        return await authRepository.CreateAsync(newAuth, cancellationToken);
    }

    public async Task<Auth> Update(string id, UpdateAuthDto updateDto, CancellationToken cancellationToken)
    {
        var updates = new List<UpdateDefinition<Auth>>();

        if (updateDto.PasswordHash != null)
            updates.Add(Builders<Auth>.Update.Set(a => a.PasswordHash, updateDto.PasswordHash));

        if (updateDto.Token != null)
            updates.Add(Builders<Auth>.Update.Set(a => a.Token, updateDto.Token));

        if (updates.Count == 0)
            throw new ArgumentException("No valid fields to update.");

        updates.Add(Builders<Auth>.Update.Set(a => a.UpdatedAt, DateTime.UtcNow));

        var updateDef = Builders<Auth>.Update.Combine(updates);

        var result = await authRepository.UpdateAsync(id, updateDef, cancellationToken);
        return result;
    }

    public async Task<bool> Delete(string id, CancellationToken cancellationToken)
    {
        return await authRepository.DeleteAsync(id, cancellationToken);
    }
}
