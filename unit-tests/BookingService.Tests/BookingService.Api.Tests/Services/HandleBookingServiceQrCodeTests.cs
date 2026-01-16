namespace BookingService.Api.Tests.Services;

using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Tests.Helpers;
using Ems.Common.Services.Tasks;
using Ems.Common.Services.Tasks.Messages;
using FluentAssertions;
using Moq;
using Xunit;

public class HandleBookingServiceQrCodeTests
{
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockTaskQueue;
    private readonly Mock<ITaskQueue<EmailNotificationTaskMessage>> _mockEmailTaskQueue;
    private readonly HandleBookingService _service;

    public HandleBookingServiceQrCodeTests()
    {
        _mockRepository = new Mock<IBookingRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        _mockEmailTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage>>();
        _service = new HandleBookingService(_mockRepository.Object, _mockQrCodeRepository.Object, _mockTaskQueue.Object, _mockEmailTaskQueue.Object);
    }

    [Fact]
    public async Task GetById_WithExistingQrCode_ShouldReturnBookingDtoWithQrCodeData()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        var qrCode = new QrCode
        {
            Id = "507f1f77bcf86cd799439012",
            BookingId = "507f1f77bcf86cd799439011",
            QrCodeData = [1, 2, 3, 4, 5],
            CreatedAt = DateTime.UtcNow
        };
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(qrCode);

        var result = await _service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<BookingDto>();
        result.Id.Should().Be(bookingEntity.Id);
        result.Status.Should().Be(bookingEntity.Status);
        result.CreatedAt.Should().Be(bookingEntity.CreatedAt);
        result.UpdatedAt.Should().Be(bookingEntity.UpdatedAt);
        result.QrCodeData.Should().NotBeNull();
        result.QrCodeData.Should().BeEquivalentTo(qrCode.QrCodeData);
    }

    [Fact]
    public async Task GetById_WithNoQrCode_ShouldReturnBookingDtoWithNullQrCodeData()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<BookingDto>();
        result.Id.Should().Be(bookingEntity.Id);
        result.Status.Should().Be(bookingEntity.Status);
        result.CreatedAt.Should().Be(bookingEntity.CreatedAt);
        result.UpdatedAt.Should().Be(bookingEntity.UpdatedAt);
        result.QrCodeData.Should().BeNull();
    }
}
