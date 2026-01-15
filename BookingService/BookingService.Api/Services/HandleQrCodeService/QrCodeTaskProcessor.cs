namespace BookingService.Api.Services;

using BookingService.Data.Models;
using BookingService.Data.Repositories;
using Ems.Common.Services.Tasks;
using Ems.Common.Services.Tasks.Messages;
using QRCoder;

public class QrCodeTaskProcessor(IQrCodeRepository qrCodeRepository, ILogger<QrCodeTaskProcessor> logger) : ITaskProcessor<QrCodeTaskMessage>
{
    public async Task ProcessAsync(QrCodeTaskMessage message, CancellationToken cancellationToken)
    {
        var bookingId = message.BookingId;
        logger.LogInformation("Generating QR code for BookingId: {BookingId}", bookingId);

        var qrContent = bookingId;

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);

        var qrCodeEntity = new QrCode
        {
            BookingId = bookingId,
            QrCodeData = qrCodeBytes,
        };

        await qrCodeRepository.CreateAsync(qrCodeEntity, cancellationToken);
        logger.LogInformation("QR code generated successfully for BookingId: {BookingId}", bookingId);
    }
}
