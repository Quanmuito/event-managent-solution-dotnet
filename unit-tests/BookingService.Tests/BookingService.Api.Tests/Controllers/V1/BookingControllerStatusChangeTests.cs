namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using TestUtilities.Helpers;
using System.Collections.Generic;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class BookingControllerStatusChangeTests : IClassFixture<BookingControllerTestFixture>
{
    private readonly BookingControllerTestFixture _fixture;

    public BookingControllerStatusChangeTests(BookingControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task Cancel_WithValidId_ShouldReturnOk()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<BookingDto>(result);
        okResult.Status.Should().Be(BookingStatus.Canceled);
    }

    [Fact]
    public async Task Cancel_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _fixture.Controller.Cancel(null!, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Cancel_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.Cancel("507f1f77bcf86cd799439999", CancellationToken.None),
            "Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Cancel_WithAlreadyCanceledBooking_ShouldThrowInvalidOperationException()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        await ControllerTestHelper.AssertExceptionThrown<InvalidOperationException>(
            async () => await _fixture.Controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None),
            "Booking is already canceled.");
    }

    [Fact]
    public async Task Cancel_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.Cancel("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Cancel_WithException_ShouldThrowException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None),
            "Database error");
    }

    [Fact]
    public async Task Confirm_WithValidId_ShouldReturnOk()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var confirmedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        confirmedBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmedBooking);

        var result = await _fixture.Controller.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        var okResult = ControllerTestHelper.AssertOkResult<BookingDto>(result);
        okResult.Status.Should().Be(BookingStatus.Registered);
    }

    [Fact]
    public async Task Confirm_WithNullId_ShouldReturnBadRequest()
    {
        var result = await _fixture.Controller.Confirm(null!, CancellationToken.None);

        ControllerTestHelper.AssertBadRequest(result);
    }

    [Fact]
    public async Task Confirm_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        await ControllerTestHelper.AssertExceptionThrown<KeyNotFoundException>(
            async () => await _fixture.Controller.Confirm("507f1f77bcf86cd799439999", CancellationToken.None),
            "Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Confirm_WithNonQueuePendingStatus_ShouldThrowInvalidOperationException()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        await ControllerTestHelper.AssertExceptionThrown<InvalidOperationException>(
            async () => await _fixture.Controller.Confirm("507f1f77bcf86cd799439011", CancellationToken.None),
            "Only bookings with QueuePending status can be confirmed.");
    }

    [Fact]
    public async Task Confirm_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        await ControllerTestHelper.AssertExceptionThrown<FormatException>(
            async () => await _fixture.Controller.Confirm("invalid-id", CancellationToken.None),
            "Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Confirm_WithException_ShouldThrowException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await ControllerTestHelper.AssertExceptionThrown<Exception>(
            async () => await _fixture.Controller.Confirm("507f1f77bcf86cd799439011", CancellationToken.None),
            "Database error");
    }
}
