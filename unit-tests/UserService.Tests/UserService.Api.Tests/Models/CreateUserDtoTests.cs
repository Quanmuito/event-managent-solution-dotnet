namespace UserService.Api.Tests.Models;

using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using UserService.Api.Models;
using UserService.Data.Utils;
using UserService.Tests.Helpers;
using Xunit;

public class CreateUserDtoTests
{
    [Fact]
    public void CreateUserDto_WithValidData_ShouldBeValid()
    {
        var dto = TestDataBuilder.CreateValidCreateUserDto();

        var validationResults = ValidateModel(dto);

        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateUserDto_WithMissingEmail_ShouldBeInvalid()
    {
        var dto = new CreateUserDto
        {
            Email = null!,
            Phone = "+1234567890"
        };

        var validationResults = ValidateModel(dto);

        validationResults.Should().Contain(v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CreateUserDto_WithInvalidEmail_ShouldBeInvalid()
    {
        var dto = new CreateUserDto
        {
            Email = "invalid-email",
            Phone = "+1234567890"
        };

        var validationResults = ValidateModel(dto);

        validationResults.Should().Contain(v => v.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CreateUserDto_WithNoRolesSet_ShouldDefaultToUserRole()
    {
        var dto = new CreateUserDto
        {
            Email = "valid@example.com"
        };

        dto.Roles.Should().Equal([UserRoles.USER]);
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}
