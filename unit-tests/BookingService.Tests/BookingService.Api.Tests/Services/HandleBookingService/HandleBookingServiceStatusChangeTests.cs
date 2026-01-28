namespace BookingService.Api.Tests.Services.HandleBookingService;

using BookingService.Api.Messages;
using BookingService.Data.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleBookingServiceStatusChangeTests : IClassFixture<HandleBookingServiceTestFixture>
{
    private readonly HandleBookingServiceTestFixture _fixture;

    public HandleBookingServiceStatusChangeTests(HandleBookingServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Theory]
    [InlineData("registered")]
    [InlineData("queue_enrolled")]
    [InlineData("queue_pending")]
    public async Task Cancel_WithValidStatus_ShouldCancelSuccessfully(string initialStatus)
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", initialStatus);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<MongoDB.Driver.UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);
        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _fixture.Service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockRepository.Verify(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithAlreadyCanceledBooking_ShouldThrowInvalidOperationException()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Booking is already canceled."));

        var act = async () => await _fixture.Service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Booking is already canceled.");
    }

    [Fact]
    public async Task Cancel_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.CancelAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.Cancel("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Cancel_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.CancelAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Cancel("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Confirm_WithValidId_ShouldConfirmAndReturnBooking()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var confirmedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        confirmedBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<MongoDB.Driver.UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmedBooking);
        _fixture.MockRepository.Setup(x => x.ConfirmAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmedBooking);

        var result = await _fixture.Service.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Registered);
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockRepository.Verify(x => x.ConfirmAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
        _fixture.MockQrCodeTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<QrCodeTaskMessage>(m => m.BookingId == "507f1f77bcf86cd799439011"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Confirm_WithNonQueuePendingStatus_ShouldThrowInvalidOperationException()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);

        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _fixture.MockRepository.Setup(x => x.ConfirmAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Only bookings with QueuePending status can be confirmed."));

        var act = async () => await _fixture.Service.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only bookings with QueuePending status can be confirmed.");
    }

    [Fact]
    public async Task Confirm_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.ConfirmAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.Confirm("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Confirm_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.ConfirmAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Confirm("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }
}
