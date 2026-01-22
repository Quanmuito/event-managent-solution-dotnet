namespace BookingService.Api.Tests.Services.HandleQrCodeService;

using BookingService.Api.Services;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Api.Messages;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class QrCodeTaskProcessorTests
{
    private readonly Mock<IQrCodeRepository> _mockRepository;
    private readonly Mock<ILogger<QrCodeTaskProcessor>> _mockLogger;
    private readonly QrCodeTaskProcessor _processor;

    public QrCodeTaskProcessorTests()
    {
        _mockRepository = new Mock<IQrCodeRepository>();
        _mockLogger = new Mock<ILogger<QrCodeTaskProcessor>>();
        _processor = new QrCodeTaskProcessor(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithValidMessage_ShouldGenerateAndSaveQrCode()
    {
        var bookingId = "507f1f77bcf86cd799439011";
        var message = new QrCodeTaskMessage(bookingId);
        QrCode? capturedQrCode = null;

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<QrCode>(), It.IsAny<CancellationToken>()))
            .Callback<QrCode, CancellationToken>((qr, ct) => capturedQrCode = qr)
            .ReturnsAsync((QrCode qr, CancellationToken ct) => qr);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockRepository.Verify(x => x.CreateAsync(
            It.Is<QrCode>(q => q.BookingId == bookingId && q.QrCodeData.Length > 0),
            It.IsAny<CancellationToken>()), Times.Once);

        capturedQrCode.Should().NotBeNull();
        capturedQrCode!.BookingId.Should().Be(bookingId);
        capturedQrCode.QrCodeData.Should().NotBeEmpty();
        capturedQrCode.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ProcessAsync_ShouldGenerateValidQrCodeData()
    {
        var bookingId = "507f1f77bcf86cd799439011";
        var message = new QrCodeTaskMessage(bookingId);
        QrCode? capturedQrCode = null;

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<QrCode>(), It.IsAny<CancellationToken>()))
            .Callback<QrCode, CancellationToken>((qr, ct) => capturedQrCode = qr)
            .ReturnsAsync((QrCode qr, CancellationToken ct) => qr);

        await _processor.ProcessAsync(message, CancellationToken.None);

        capturedQrCode.Should().NotBeNull();
        capturedQrCode!.QrCodeData.Should().NotBeEmpty();
        capturedQrCode.QrCodeData.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task ProcessAsync_WithDifferentBookingIds_ShouldGenerateDifferentQrCodes()
    {
        var bookingId1 = "507f1f77bcf86cd799439011";
        var bookingId2 = "507f1f77bcf86cd799439012";
        var message1 = new QrCodeTaskMessage(bookingId1);
        var message2 = new QrCodeTaskMessage(bookingId2);

        QrCode? capturedQrCode1 = null;
        QrCode? capturedQrCode2 = null;

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<QrCode>(), It.IsAny<CancellationToken>()))
            .Callback<QrCode, CancellationToken>((qr, ct) =>
            {
                if (qr.BookingId == bookingId1)
                    capturedQrCode1 = qr;
                else if (qr.BookingId == bookingId2)
                    capturedQrCode2 = qr;
            })
            .ReturnsAsync((QrCode qr, CancellationToken ct) => qr);

        await _processor.ProcessAsync(message1, CancellationToken.None);
        await _processor.ProcessAsync(message2, CancellationToken.None);

        capturedQrCode1.Should().NotBeNull();
        capturedQrCode2.Should().NotBeNull();
        capturedQrCode1!.QrCodeData.Should().NotBeEquivalentTo(capturedQrCode2!.QrCodeData);
    }

    [Fact]
    public async Task ProcessAsync_ShouldLogInformationMessages()
    {
        var bookingId = "507f1f77bcf86cd799439011";
        var message = new QrCodeTaskMessage(bookingId);

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<QrCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode qr, CancellationToken ct) => qr);

        await _processor.ProcessAsync(message, CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating QR code")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("QR code generated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
