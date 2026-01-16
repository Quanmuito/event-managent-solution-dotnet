namespace BookingService.Api.Tests.Services;

using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using DatabaseService.Exceptions;
using Ems.Common.Services.Tasks;
using Ems.Common.Services.Tasks.Messages;
using BookingService.Api.Messages;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using Xunit;

public class HandleBookingServiceTests
{
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockTaskQueue;
    private readonly Mock<ITaskQueue<EmailNotificationTaskMessage>> _mockEmailTaskQueue;
    private readonly HandleBookingService _service;

    public HandleBookingServiceTests()
    {
        _mockRepository = new Mock<IBookingRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        _mockEmailTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage>>();
        _service = new HandleBookingService(_mockRepository.Object, _mockQrCodeRepository.Object, _mockTaskQueue.Object, _mockEmailTaskQueue.Object);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnBookingDto()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<BookingDto>();
        AssertBookingDtoMatchesBooking(result, bookingEntity);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ShouldThrowNotFoundException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

        var act = async () => await _service.GetById("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task GetById_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _service.GetById("invalid-id", CancellationToken.None);

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

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        result.Status.Should().Be(BookingStatus.Registered);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockRepository.Verify(x => x.CreateAsync(It.Is<Booking>(b =>
            b.Status == BookingStatus.Registered), It.IsAny<CancellationToken>()), Times.Once);
        _mockTaskQueue.Verify(x => x.EnqueueAsync(
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

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });

        var result = await _service.Create(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("507f1f77bcf86cd799439011");
        result.Status.Should().Be(BookingStatus.QueueEnrolled);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockRepository.Verify(x => x.CreateAsync(It.Is<Booking>(b =>
            b.Status == BookingStatus.QueueEnrolled), It.IsAny<CancellationToken>()), Times.Once);
        _mockTaskQueue.Verify(x => x.EnqueueAsync(
            It.IsAny<QrCodeTaskMessage>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenBookingIdIsNull_ShouldThrowInvalidOperationException()
    {
        var dto = TestDataBuilder.CreateValidCreateBookingDto();
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = null;
                return b;
            });

        var act = async () => await _service.Create(dto, CancellationToken.None);

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

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        var result = await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(updateDto.Status);
        result.UpdatedAt.Should().NotBeNull();
        _mockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()), Times.Once);
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

        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        var result = await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.UpdatedAt.Should().NotBeNull();
        _mockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithNoValidFields_ShouldThrowArgumentException()
    {
        var updateDto = new UpdateBookingDto();

        var act = async () => await _service.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("No valid fields to update.");
    }

    [Fact]
    public async Task Update_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439999", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

        var act = async () => await _service.Update("507f1f77bcf86cd799439999", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Update_WithInvalidFormatId_ShouldThrowFormatException()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        _mockRepository.Setup(x => x.UpdateAsync("invalid-id", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _service.Update("invalid-id", updateDto, CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteAndReturnTrue()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.Delete("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ShouldThrowNotFoundException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

        var act = async () => await _service.Delete("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Delete_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _service.Delete("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    [Fact]
    public async Task Cancel_WithValidId_ShouldCancelAndReturnBooking()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        result.UpdatedAt.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithAlreadyCanceledBooking_ShouldThrowInvalidOperationException()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        var act = async () => await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Booking is already canceled.");
    }

    [Fact]
    public async Task Cancel_WithQueueEnrolledStatus_ShouldCancelSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueueEnrolled);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        result.UpdatedAt.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithQueuePendingStatus_ShouldCancelSuccessfully()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _service.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(BookingStatus.Canceled);
        result.UpdatedAt.Should().NotBeNull();
        _mockRepository.Verify(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithNonExistentId_ShouldThrowNotFoundException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439999", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Bookings", "507f1f77bcf86cd799439999"));

        var act = async () => await _service.Cancel("507f1f77bcf86cd799439999", CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Bookings with ID '507f1f77bcf86cd799439999' was not found.");
    }

    [Fact]
    public async Task Cancel_WithInvalidFormatId_ShouldThrowFormatException()
    {
        _mockRepository.Setup(x => x.GetByIdAsync("invalid-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("Invalid ObjectId format: invalid-id"));

        var act = async () => await _service.Cancel("invalid-id", CancellationToken.None);

        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Invalid ObjectId format: invalid-id");
    }

    private static void AssertBookingDtoMatchesBooking(BookingDto dto, Booking bookingEntity)
    {
        dto.Should().NotBeNull();
        dto.Id.Should().Be(bookingEntity.Id);
        dto.Status.Should().Be(bookingEntity.Status);
        dto.CreatedAt.Should().Be(bookingEntity.CreatedAt);
        dto.UpdatedAt.Should().Be(bookingEntity.UpdatedAt);
    }
}
