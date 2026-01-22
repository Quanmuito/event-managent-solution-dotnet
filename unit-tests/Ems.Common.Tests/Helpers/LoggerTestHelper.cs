namespace Ems.Common.Tests.Helpers;

using Microsoft.Extensions.Logging;
using Moq;

public static class LoggerTestHelper
{
    public static void VerifyErrorLogged<T>(Mock<ILogger<T>> mockLogger, Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            times);
    }

    public static void VerifyErrorLogged<T>(Mock<ILogger<T>> mockLogger)
    {
        VerifyErrorLogged(mockLogger, Times.Once());
    }
}
