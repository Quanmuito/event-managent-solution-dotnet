namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Tests.Helpers;
using TestUtilities.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

public class BookingControllerQrCodeTests : IClassFixture<BookingControllerTestFixture>
{
    private readonly BookingControllerTestFixture _fixture;

    public BookingControllerQrCodeTests(BookingControllerTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetMocks();
    }
    [Fact]
    public async Task GetById_WithExistingQrCode_ShouldReturnOkWithQrCodeData()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        var qrCode = new QrCode
        {
            Id = "507f1f77bcf86cd799439012",
            BookingId = "507f1f77bcf86cd799439011",
            QrCodeData = [1, 2, 3, 4, 5],
            CreatedAt = DateTime.UtcNow
        };
        var bookingDto = new BookingDto(bookingEntity)
        {
            QrCodeData = qrCode.QrCodeData
        };
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(qrCode);

        var result = await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        var returnedDto = ControllerTestHelper.AssertOkResult<BookingDto>(result);
        returnedDto.QrCodeData.Should().NotBeNull();
        returnedDto.QrCodeData.Should().BeEquivalentTo(qrCode.QrCodeData);
    }

    [Fact]
    public async Task GetById_WithNoQrCode_ShouldReturnOkWithNullQrCodeData()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        _fixture.MockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _fixture.MockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _fixture.Controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        var returnedDto = ControllerTestHelper.AssertOkResult<BookingDto>(result);
        returnedDto.QrCodeData.Should().BeNull();
    }
}
