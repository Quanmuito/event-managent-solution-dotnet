namespace EventService.Data.Tests.Exceptions;

using EventService.Data.Exceptions;
using FluentAssertions;
using Xunit;

public class NotFoundExceptionTests
{
    [Fact]
    public void NotFoundException_ShouldHaveCorrectMessage()
    {
        var collectionName = "Events";
        var id = "507f1f77bcf86cd799439011";
        var exception = new NotFoundException(collectionName, id);

        exception.Message.Should().Be($"Events with ID '507f1f77bcf86cd799439011' was not found.");
    }

    [Fact]
    public void NotFoundException_ShouldBeExceptionType()
    {
        var exception = new NotFoundException("TestCollection", "123");

        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void NotFoundException_WithDifferentCollectionName_ShouldHaveCorrectMessage()
    {
        var collectionName = "Users";
        var id = "507f1f77bcf86cd799439999";
        var exception = new NotFoundException(collectionName, id);

        exception.Message.Should().Be($"Users with ID '507f1f77bcf86cd799439999' was not found.");
    }
}
