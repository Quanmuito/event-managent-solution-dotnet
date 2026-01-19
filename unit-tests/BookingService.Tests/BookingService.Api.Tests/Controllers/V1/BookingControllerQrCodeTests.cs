namespace BookingService.Api.Tests.Controllers.V1;

using BookingService.Api.Controllers.V1;
using BookingService.Api.Models;
using BookingService.Api.Services;
using BookingService.Api.Messages;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Tests.Helpers;
using Ems.Common.Messages;
using Ems.Common.Services.Tasks;
using EventService.Data.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BookingControllerQrCodeTests
{
    private readonly Mock<ILogger<BookingController>> _mockLogger;
    private readonly Mock<IBookingRepository> _mockRepository;
    private readonly Mock<IQrCodeRepository> _mockQrCodeRepository;
    private readonly Mock<IEventRepository> _mockEventRepository;
    private readonly Mock<ITaskQueue<QrCodeTaskMessage>> _mockQrCodeTaskQueue;
    private readonly Mock<ITaskQueue<EmailNotificationTaskMessage<BookingDto>>> _mockEmailNotificationTaskQueue;
    private readonly Mock<ITaskQueue<PhoneNotificationTaskMessage<BookingDto>>> _mockPhoneNotificationTaskQueue;
    private readonly HandleBookingService _bookingService;
    private readonly BookingController _controller;

    public BookingControllerQrCodeTests()
    {
        _mockLogger = new Mock<ILogger<BookingController>>();
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
        _controller = new BookingController(_mockLogger.Object, _bookingService);
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
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(qrCode);

        var result = await _controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedDto = okResult!.Value as BookingDto;
        returnedDto.Should().NotBeNull();
        returnedDto!.QrCodeData.Should().NotBeNull();
        returnedDto.QrCodeData.Should().BeEquivalentTo(qrCode.QrCodeData);
    }

    [Fact]
    public async Task GetById_WithNoQrCode_ShouldReturnOkWithNullQrCodeData()
    {
        var bookingEntity = TestDataBuilder.CreateBooking("507f1f77bcf86cd799439011");
        var bookingDto = new BookingDto(bookingEntity);
        _mockRepository.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookingEntity);
        _mockQrCodeRepository.Setup(x => x.GetByBookingIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync((QrCode?)null);

        var result = await _controller.GetById("507f1f77bcf86cd799439011", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedDto = okResult!.Value as BookingDto;
        returnedDto.Should().NotBeNull();
        returnedDto!.QrCodeData.Should().BeNull();
    }
}
