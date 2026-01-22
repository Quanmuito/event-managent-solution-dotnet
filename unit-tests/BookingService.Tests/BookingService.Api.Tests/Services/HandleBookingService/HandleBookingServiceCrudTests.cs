namespace BookingService.Api.Tests.Services.HandleBookingService;

using BookingService.Api.Messages;
using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using EventService.Data.Models;
using TestUtilities.Helpers;
using System.Collections.Generic;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleBookingServiceCrudTests : IClassFixture<HandleBookingServiceTestFixture>
{
    private readonly HandleBookingServiceTestFixture _fixture;

    public HandleBookingServiceCrudTests(HandleBookingServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnBookingDto()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _fixture.Service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<BookingDto>();
        ServiceTestHelper.AssertDtoMatchesEntity(result, bookingEntity, "Id", "Status", "CreatedAt", "UpdatedAt");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.GetById("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Create_WithRegisteredStatus_ShouldCreateSuccessfully()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        dto.Status = BookingStatus.Registered;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = dto.Status;

        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(dto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = dto.EventId });
        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        result.Status.Should().Be(BookingStatus.Registered);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _fixture.MockRepository.Verify(x => x.CreateAsync(It.Is<Booking>(b =>
            b.Status == BookingStatus.Registered), It.IsAny<CancellationToken>()), Times.Once);
        _fixture.MockQrCodeTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<QrCodeTaskMessage>(m => m.BookingId == "507f1f77bcf86cd799439011"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithQueueEnrolledStatus_ShouldCreateSuccessfully()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        dto.Status = BookingStatus.QueueEnrolled;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = dto.Status;

        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(dto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = dto.EventId });
        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _fixture.Service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        result.Status.Should().Be(BookingStatus.QueueEnrolled);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _fixture.MockRepository.Verify(x => x.CreateAsync(It.Is<Booking>(b =>
            b.Status == BookingStatus.QueueEnrolled), It.IsAny<CancellationToken>()), Times.Once);
        _fixture.MockQrCodeTaskQueue.Verify(x => x.EnqueueAsync(
            It.IsAny<QrCodeTaskMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenEventDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(dto.EventId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Events with ID '{dto.EventId}' was not found."));

        var act = async () => await _fixture.Service.Create(dto, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Events with ID '{dto.EventId}' was not found.");
    }

    [Fact]
    public async Task Create_WhenBookingIdIsNull_ShouldThrowInvalidOperationException()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        _fixture.MockEventRepository.Setup(x => x.GetByIdAsync(dto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = dto.EventId });
        _fixture.MockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = null;
                return b;
            });

        var act = async () => await _fixture.Service.Create(dto, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to create booking.");
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldUpdateAndReturnBooking()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        var updatedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        updatedBooking.Status = updateDto.Status!;
        updatedBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(updateDto.Status);
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNullStatusButOtherFields_ShouldUpdateFields()
    {
        var updateDto = new UpdateBookingDto
        {
            Status = null,
            Name = "Updated Name"
        };
        var existingBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        existingBooking.Name = "Updated Name";
        existingBooking.UpdatedAt = DateTime.UtcNow;

        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        var result = await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.UpdatedAt.Should().NotBeNull();
        _fixture.MockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNoValidFields_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateBookingDto();

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowKeyNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _fixture.MockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439999", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Bookings with ID '507f1f77bcf86cd799439999' was not found."));

        var act = async () => await _fixture.Service.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _fixture.MockRepository.Setup(x => x.UpdateAsync("invalid-id", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Update("invalid-id", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
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

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldReturnFalse()
    {
        _fixture.MockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _fixture.Service.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        result.Should().BeFalse();
        _fixture.MockRepository.Verify(x => x.DeleteAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _fixture.MockRepository.Setup(x => x.DeleteAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _fixture.Service.Delete("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }
}
