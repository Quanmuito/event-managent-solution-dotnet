namespace TestUtilities.Helpers;

using System.ComponentModel.DataAnnotations;

public static class ValidationTestHelper
{
    public static ValidationContext CreateValidationContext<T>(T instance)
    {
        return new ValidationContext(instance!);
    }

    public static (bool IsValid, List<ValidationResult> Results) ValidateObject<T>(T instance)
    {
        var validationContext = new ValidationContext(instance!);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance!, validationContext, results, true);
        if (instance is IValidatableObject validatable)
        {
            var customResults = validatable.Validate(validationContext);
            results.AddRange(customResults);
        }
        var isValid = results.Count == 0;
        return (isValid, results);
    }
}
