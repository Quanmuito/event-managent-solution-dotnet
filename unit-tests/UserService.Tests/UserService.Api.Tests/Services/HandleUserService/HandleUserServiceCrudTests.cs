namespace UserService.Api.Tests.Services.HandleUserService;

using FluentAssertions;
using MongoDB.Driver;
using Moq;
using TestUtilities.Helpers;
using UserService.Api.Models;
using UserService.Data.Models;
using UserService.Tests.Helpers;
using Xunit;

public class HandleUserServiceCrudTests : IClassFixture<HandleUserServiceTestFixture>
{
    private readonly HandleUserServiceTestFixture _fixture;

    public HandleUserServiceCrudTests(HandleUserServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnUserDto()
    {
        var user = TestDataBuilder.CreateDefaultUser();
        _fixture.MockRepository.Setup(x => x.GetActiveByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _fixture.Service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<UserDto>();
        ServiceTestHelper.AssertDtoMatchesEntity(result, user, "Id", "Email", "Phone", "IsVerified", "IsDeleted", "DeletedAt", "DeletedBy", "DeletedReason", "CreatedAt", "UpdatedAt");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.GetActiveByIdAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Users with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Users with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.GetActiveByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.GetById("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateAndReturnUser()
    {
        var dto = TestDataBuilder.CreateValidCreateUserDto();

        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken ct) => u);

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be(dto.Email);
        result.Phone.Should().Be(dto.Phone);
        result.IsVerified.Should().Be(dto.IsVerified);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _fixture.MockRepository.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Email == dto.Email &&
            u.Phone == dto.Phone &&
            u.IsVerified == dto.IsVerified), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldUpdateAndReturnUser()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateUserDto();
        var updatedUser = TestDataBuilder.CreateDefaultUser();
        updatedUser.Email = updateDto.Email!;
        updatedUser.Phone = updateDto.Phone!;
        updatedUser.IsVerified = updateDto.IsVerified!.Value;
        updatedUser.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<User>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be(updateDto.Email);
        result.Phone.Should().Be(updateDto.Phone);
        result.IsVerified.Should().Be(updateDto.IsVerified!.Value);
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<User>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNoValidFields_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateUserDto();

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateUserDto();
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439999", It.IsAny<UpdateDefinition<User>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Users with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Users with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateUserDto();
        _fixture.MockRepository.Setup(x => x.UpdateAsync("invalid-id", It.IsAny<UpdateDefinition<User>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Update("invalid-id", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldSoftDeleteAndReturnTrue()
    {
        _fixture.MockRepository.Setup(x => x.SoftDeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _fixture.Service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeTrue();
        _fixture.MockRepository.Verify(x => x.SoftDeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnFalse()
    {
        _fixture.MockRepository.Setup(x => x.SoftDeleteAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _fixture.Service.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.SoftDeleteAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Delete("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }
}
