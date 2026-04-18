namespace AuthService.Api.Tests.Models;

using AuthService.Api.Models;
using AuthService.Tests.Helpers;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class CreateAuthDtoTests
{
    [Fact]
    public void CreateAuthDto_WithValidData_ShouldBeValid()
    {
        var dto = TestDataBuilder.CreateValidCreateAuthDto();

        var validationResults = ValidateModel(dto);

        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateAuthDto_WithMissingUserId_ShouldBeInvalid()
    {
        var dto = new CreateAuthDto
        {
            UserId = null!,
            PasswordHash = "hashed-password-12345",
            Token = "test-token"
        };

        var validationResults = ValidateModel(dto);

        validationResults.Should().Contain(v => v.MemberNames.Contains("UserId"));
    }

    [Fact]
    public void CreateAuthDto_WithMissingToken_ShouldBeInvalid()
    {
        var dto = new CreateAuthDto
        {
            UserId = "507f1f77bcf86cd799439011",
            PasswordHash = "hashed-password-12345",
            Token = null!
        };

        var validationResults = ValidateModel(dto);

        validationResults.Should().Contain(v => v.MemberNames.Contains("Token"));
    }

    [Fact]
    public void CreateAuthDto_WithMissingPasswordHash_ShouldBeInvalid()
    {
        var dto = new CreateAuthDto
        {
            UserId = "507f1f77bcf86cd799439011",
            PasswordHash = null!,
            Token = "test-token"
        };

        var validationResults = ValidateModel(dto);

        validationResults.Should().Contain(v => v.MemberNames.Contains("PasswordHash"));
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}
