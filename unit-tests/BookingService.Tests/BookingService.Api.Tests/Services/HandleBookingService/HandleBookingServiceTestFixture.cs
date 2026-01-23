namespace BookingService.Api.Tests.Services.HandleBookingService;

using BookingService.Api.Messages;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Data.Repositories;
using Ems.Common.Services.Tasks;
using EventService.Data.Repositories;
using NotificationService.Common.Messages;
using Moq;

public class HandleBookingServiceTestFixture : IDisposable
{
    public Mock<IBookingRepository> MockRepository { get; }
    public Mock<IQrCodeRepository> MockQrCodeRepository { get; }
    public Mock<IEventRepository> MockEventRepository { get; }
    public Mock<ITaskQueue<QrCodeTaskMessage>> MockQrCodeTaskQueue { get; }
    public Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>> MockEmailNotificationTaskQueue { get; }
    public Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>> MockPhoneNotificationTaskQueue { get; }
    public HandleBookingService Service { get; }

    public HandleBookingServiceTestFixture()
    {
        MockRepository = new Mock<IBookingRepository>();
        MockQrCodeRepository = new Mock<IQrCodeRepository>();
        MockEventRepository = new Mock<IEventRepository>();
        MockQrCodeTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        MockEmailNotificationTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>>();
        MockPhoneNotificationTaskQueue = new Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>>();
        Service = new HandleBookingService(
            MockRepository.Object,
            MockQrCodeRepository.Object,
            MockEventRepository.Object,
            MockQrCodeTaskQueue.Object,
            MockEmailNotificationTaskQueue.Object,
            MockPhoneNotificationTaskQueue.Object);
    }

    public void ResetMocks()
    {
        MockRepository.Reset();
        MockQrCodeRepository.Reset();
        MockEventRepository.Reset();
        MockQrCodeTaskQueue.Reset();
        MockEmailNotificationTaskQueue.Reset();
        MockPhoneNotificationTaskQueue.Reset();
    }

    public void VerifyNotifications(string bookingId, string operation)
    {
        MockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == bookingId && m.Operation == operation),
            It.IsAny<CancellationToken>()), Times.Once);
        MockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == bookingId && m.Operation == operation),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
    }
}
