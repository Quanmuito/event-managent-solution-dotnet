namespace AuthService.Api.Services;

using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Data.Repositories;
using MongoDB.Driver;

public class HandleUserService(IUserRepository userRepository)
{
    public async Task<List<UserDto>> Search(string? query, CancellationToken cancellationToken)
    {
        var users = await userRepository.SearchAsync(query, cancellationToken);
        return [.. users.Select(u => new UserDto(u))];
    }

    public async Task<UserDto> GetById(string id, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return new UserDto(user);
    }

    public async Task<User> Create(CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var newUser = new User
        {
            Email = createDto.Email,
            Phone = createDto.Phone,
            IsVerified = createDto.IsVerified,
            CreatedAt = DateTime.UtcNow
        };
        return await userRepository.CreateAsync(newUser, cancellationToken);
    }

    public async Task<User> Update(string id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var updates = new List<UpdateDefinition<User>>();

        if (updateDto.Email != null)
            updates.Add(Builders<User>.Update.Set(u => u.Email, updateDto.Email));

        if (updateDto.Phone != null)
            updates.Add(Builders<User>.Update.Set(u => u.Phone, updateDto.Phone));

        if (updateDto.IsVerified.HasValue)
            updates.Add(Builders<User>.Update.Set(u => u.IsVerified, updateDto.IsVerified.Value));

        if (updates.Count == 0)
            throw new ArgumentException("No valid fields to update.");

        updates.Add(Builders<User>.Update.Set(u => u.UpdatedAt, DateTime.UtcNow));

        var updateDef = Builders<User>.Update.Combine(updates);

        var result = await userRepository.UpdateAsync(id, updateDef, cancellationToken);
        return result;
    }

    public async Task<bool> Delete(string id, CancellationToken cancellationToken)
    {
        var updates = new List<UpdateDefinition<User>>
        {
            Builders<User>.Update.Set(u => u.IsDeleted, true),
            Builders<User>.Update.Set(u => u.DeletedAt, DateTime.UtcNow),
            Builders<User>.Update.Set(u => u.UpdatedAt, DateTime.UtcNow)
        };

        var updateDef = Builders<User>.Update.Combine(updates);
        var result = await userRepository.UpdateAsync(id, updateDef, cancellationToken);
        return result != null;
    }
}
