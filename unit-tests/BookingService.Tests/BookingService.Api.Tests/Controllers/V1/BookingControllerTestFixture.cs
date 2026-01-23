namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Controllers.V1;
using BookingService.Api.Messages;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Data.Repositories;
using Ems.Common.Services.Tasks;
using EventService.Data.Repositories;
using NotificationService.Common.Messages;
using Moq;

public class BookingControllerTestFixture : IDisposable
{
    public Mock<IBookingRepository> MockRepository { get; }
    public Mock<IQrCodeRepository> MockQrCodeRepository { get; }
    public Mock<IEventRepository> MockEventRepository { get; }
    public Mock<ITaskQueue<QrCodeTaskMessage>> MockQrCodeTaskQueue { get; }
    public Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>> MockEmailNotificationTaskQueue { get; }
    public Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>> MockPhoneNotificationTaskQueue { get; }
    public HandleBookingService BookingService { get; }
    public BookingController Controller { get; }

    public BookingControllerTestFixture()
    {
        MockRepository = new Mock<IBookingRepository>();
        MockQrCodeRepository = new Mock<IQrCodeRepository>();
        MockEventRepository = new Mock<IEventRepository>();
        MockQrCodeTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        MockEmailNotificationTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>>();
        MockPhoneNotificationTaskQueue = new Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>>();
        BookingService = new HandleBookingService(
            MockRepository.Object,
            MockQrCodeRepository.Object,
            MockEventRepository.Object,
            MockQrCodeTaskQueue.Object,
            MockEmailNotificationTaskQueue.Object,
            MockPhoneNotificationTaskQueue.Object);
        Controller = new BookingController(BookingService);
    }

    public void ResetMocks()
    {
        MockRepository.Reset();
        MockQrCodeRepository.Reset();
        MockEventRepository.Reset();
        MockQrCodeTaskQueue.Reset();
        MockEmailNotificationTaskQueue.Reset();
        MockPhoneNotificationTaskQueue.Reset();
        Controller.ModelState.Clear();
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
