namespace AuthService.Api.Services;

using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Data.Repositories;
using MongoDB.Driver;

public class HandleAuthService(IAuthRepository authRepository)
{
    public async Task<List<AuthDto>> Search(string? query, CancellationToken cancellationToken)
    {
        var auths = await authRepository.GetAllAsync(cancellationToken);
        return [.. auths.Select(a => new AuthDto(a))];
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
            Token = createDto.Token,
            CreatedAt = DateTime.UtcNow
        };
        return await authRepository.CreateAsync(newAuth, cancellationToken);
    }

    public async Task<Auth> Update(string id, UpdateAuthDto updateDto, CancellationToken cancellationToken)
    {
        var updates = new List<UpdateDefinition<Auth>>();

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
