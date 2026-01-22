namespace TestUtilities.Helpers;

using Microsoft.Extensions.Logging;
using Moq;

public static class LoggerTestHelper
{
    public static void VerifyLogInformation<T>(Mock<ILogger<T>> logger, string messageTemplate)
    {
        var prefix = messageTemplate.Split('{')[0];
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(prefix)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public static void VerifyLogWarning<T>(Mock<ILogger<T>> logger, string messageTemplate)
    {
        var prefix = messageTemplate.Split('{')[0];
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(prefix)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public static void VerifyLogError<T>(Mock<ILogger<T>> logger, string messageTemplate)
    {
        var prefix = messageTemplate.Split('{')[0];
        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(prefix)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public static void VerifyLog<T>(Mock<ILogger<T>> logger, LogLevel logLevel, Times times)
    {
        logger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times);
    }

    public static void VerifyLogInformation<T>(Mock<ILogger<T>> logger)
    {
        VerifyLog(logger, LogLevel.Information, Times.Exactly(1));
    }

    public static void VerifyLogError<T>(Mock<ILogger<T>> logger)
    {
        VerifyLog(logger, LogLevel.Error, Times.Exactly(1));
    }

    public static void VerifyLogDebug<T>(Mock<ILogger<T>> logger)
    {
        VerifyLog(logger, LogLevel.Debug, Times.Exactly(1));
    }
}
