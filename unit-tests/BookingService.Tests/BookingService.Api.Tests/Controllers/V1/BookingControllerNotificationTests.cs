namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Controllers.V1;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Api.Utils;
using BookingService.Api.Messages;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using BookingService.Tests.Helpers;
using Ems.Common.Messages;
using Ems.Common.Services.Tasks;
using EventService.Data.Models;
using EventService.Data.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Moq;
using Xunit;

public class BookingControllerNotificationTests
{
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockQrCodeTaskQueue;
    private readonly Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>> _mockEmailNotificationTaskQueue;
    private readonly Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>> _mockPhoneNotificationTaskQueue;
    private readonly HandleBookingService _bookingService;
    private readonly BookingController _controller;

    public BookingControllerNotificationTests()
    {
        _mockRepository = new Mock<IBookingRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockQrCodeTaskQueue = new Mock<ITaskQueue<QrCodeTaskMessage>>();
        _mockEmailNotificationTaskQueue = new Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>>();
        _mockPhoneNotificationTaskQueue = new Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>>();
        _bookingService = new HandleBookingService(
            _mockRepository.Object,
            _mockQrCodeRepository.Object,
            _mockEventRepository.Object,
            _mockQrCodeTaskQueue.Object,
            _mockEmailNotificationTaskQueue.Object,
            _mockPhoneNotificationTaskQueue.Object);
        _controller = new BookingController(_bookingService);
    }

    [Fact]
    public async Task Create_WithRegisteredStatus_ShouldTriggerNotification()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        createDto.Status = BookingStatus.Registered;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = BookingStatus.Registered;
        _mockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = createDto.EventId });
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        _mockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Registered),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Registered),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var createDto = TestDataBuilder.CreateValidCreateBookingDto();
        createDto.Status = BookingStatus.QueueEnrolled;
        var createdBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        createdBooking.Status = BookingStatus.QueueEnrolled;
        _mockEventRepository.Setup(x => x.GetByIdAsync(createDto.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = createDto.EventId });
        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking b, CancellationToken ct) =>
            {
                b.Id = "507f1f77bcf86cd799439011";
                return b;
            });
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _controller.Create(createDto, CancellationToken.None);

        result.Should().BeOfType<CreatedAtActionResult>();
        _mockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.QueueEnrolled),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.QueueEnrolled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidDto_ShouldTriggerNotification()
    {
        var updateDto = TestDataBuilder.CreateValidUpdateBookingDto();
        var updatedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        updatedBooking.Status = updateDto.Status!;
        updatedBooking.UpdatedAt = DateTime.UtcNow;
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedBooking);

        var result = await _controller.Update("507f1f77bcf86cd799439011", updateDto, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _mockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Updated),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Updated),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Confirm_WithValidId_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var confirmedBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        confirmedBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmedBooking);

        var result = await _controller.Confirm("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
        var dto = okResult.Value as BookingDto;
        dto!.Status.Should().Be(BookingStatus.Registered);
        _mockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Confirmed),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Confirmed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithValidId_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Registered);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<BookingDto>();
        _mockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithQueueEnrolledStatus_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueueEnrolled);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _mockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_WithQueuePendingStatus_ShouldTriggerNotification()
    {
        var booking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.QueuePending);
        var canceledBooking = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011", BookingStatus.Canceled);
        canceledBooking.UpdatedAt = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _mockRepository.Setup(x => x.UpdateAsync("507f1f77bcf86cd799439011", It.IsAny<UpdateDefinition<Booking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(canceledBooking);

        var result = await _controller.Cancel("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        _mockEmailNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<EmailNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockPhoneNotificationTaskQueue.Verify(x => x.EnqueueAsync(
            It.Is<PhoneNotificationTaskMessage<BookingDto>>(m => m.Data.Id == "507f1f77bcf86cd799439011" && m.Operation == BookingOperation.Canceled),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
