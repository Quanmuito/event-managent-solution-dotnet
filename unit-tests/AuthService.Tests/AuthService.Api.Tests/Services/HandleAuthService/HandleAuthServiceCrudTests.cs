namespace AuthService.Api.Tests.Services.HandleAuthService;

using AuthService.Api.Models;
using AuthService.Data.Models;
using AuthService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleAuthServiceCrudTests : IClassFixture<HandleAuthServiceTestFixture>
{
    private readonly HandleAuthServiceTestFixture _fixture;

    public HandleAuthServiceCrudTests(HandleAuthServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnAuthDto()
    {
        var auth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(auth);

        var result = await _fixture.Service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<AuthDto>();
        ServiceTestHelper.AssertDtoMatchesEntity(result, auth, "Id", "UserId", "Token", "CreatedAt", "UpdatedAt");
    }

    [Fact]
    public async Task Create_WithValidDto_ShouldCreateAndReturnAuth()
    {
        var dto = TestDataBuilder.CreateValidCreateAuthDto();
        var createdAuth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011", dto.UserId, dto.Token);

        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Auth>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Auth a, CancellationToken ct) => a);

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(dto.UserId);
        result.Token.Should().Be(dto.Token);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _fixture.MockRepository.Verify(x => x.CreateAsync(It.Is<Auth>(a =>
            a.UserId == dto.UserId &&
            a.Token == dto.Token), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldUpdateAndReturnAuth()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateAuthDto();
        var updatedAuth = TestDataBuilder.CreateAuth("507f1f77bcf86cd799439011");
        updatedAuth.Token = updateDto.Token!;
        updatedAuth.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Auth>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedAuth);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be(updateDto.Token);
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Auth>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNoValidFields_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateAuthDto();

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteAndReturnTrue()
    {
        _fixture.MockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _fixture.Service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeTrue();
        _fixture.MockRepository.Verify(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }
}
