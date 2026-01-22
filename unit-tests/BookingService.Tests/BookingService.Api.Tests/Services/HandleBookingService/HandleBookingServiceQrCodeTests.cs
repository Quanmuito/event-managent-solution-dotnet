namespace BookingService.Api.Tests.Services.HandleBookingService;

using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

public class HandleBookingServiceQrCodeTests : IClassFixture<HandleBookingServiceTestFixture>
{
    private readonly HandleBookingServiceTestFixture _fixture;

    public HandleBookingServiceQrCodeTests(HandleBookingServiceTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
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
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(qrCode);

        var result = await _fixture.Service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

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
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _fixture.Service.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<BookingDto>();
        result.Id.Should().Be(bookingEntity.Id);
        result.Status.Should().Be(bookingEntity.Status);
        result.CreatedAt.Should().Be(bookingEntity.CreatedAt);
        result.UpdatedAt.Should().Be(bookingEntity.UpdatedAt);
        result.QrCodeData.Should().BeNull();
    }
}
