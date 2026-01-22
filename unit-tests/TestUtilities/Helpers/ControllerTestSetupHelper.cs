namespace TestUtilities.Helpers;

using DatabaseService.Repositories;
using Moq;

public static class ControllerTestSetupHelper
{
    public static void SetupMockRepositoryForGetById<T>(Mock<IRepository<T>> mockRepository, T entity, string id)
        where T : class
    {
        mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    public static void SetupMockRepositoryForGetById<TRepository, T>(Mock<TRepository> mockRepository, T entity, string id)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    public static void SetupMockRepositoryForGetByIdThrows<T>(Mock<IRepository<T>> mockRepository, Exception exception)
        where T : class
    {
        mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public static void SetupMockRepositoryForGetByIdThrows<TRepository, T>(Mock<TRepository> mockRepository, Exception exception)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public static void SetupMockRepositoryForCreate<T>(Mock<IRepository<T>> mockRepository, T entity)
        where T : class
    {
        mockRepository.Setup(x => x.CreateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    public static void SetupMockRepositoryForCreate<TRepository, T>(Mock<TRepository> mockRepository, T entity)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.CreateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    public static void SetupMockRepositoryForCreateThrows<T>(Mock<IRepository<T>> mockRepository, Exception exception)
        where T : class
    {
        mockRepository.Setup(x => x.CreateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public static void SetupMockRepositoryForCreateThrows<TRepository, T>(Mock<TRepository> mockRepository, Exception exception)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.CreateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public static void SetupMockRepositoryForUpdate<T>(Mock<IRepository<T>> mockRepository, T entity, string id)
        where T : class
    {
        mockRepository.Setup(x => x.UpdateAsync(id, It.IsAny<MongoDB.Driver.UpdateDefinition<T>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    public static void SetupMockRepositoryForUpdate<TRepository, T>(Mock<TRepository> mockRepository, T entity, string id)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.UpdateAsync(id, It.IsAny<MongoDB.Driver.UpdateDefinition<T>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
    }

    public static void SetupMockRepositoryForUpdateThrows<T>(Mock<IRepository<T>> mockRepository, Exception exception)
        where T : class
    {
        mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<MongoDB.Driver.UpdateDefinition<T>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public static void SetupMockRepositoryForUpdateThrows<TRepository, T>(Mock<TRepository> mockRepository, Exception exception)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<MongoDB.Driver.UpdateDefinition<T>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public static void SetupMockRepositoryForDelete<T>(Mock<IRepository<T>> mockRepository, bool result)
        where T : class
    {
        mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    public static void SetupMockRepositoryForDelete<TRepository, T>(Mock<TRepository> mockRepository, bool result)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    public static void SetupMockRepositoryForDeleteThrows<T>(Mock<IRepository<T>> mockRepository, Exception exception)
        where T : class
    {
        mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public static void SetupMockRepositoryForDeleteThrows<TRepository, T>(Mock<TRepository> mockRepository, Exception exception)
        where T : class
        where TRepository : class, IRepository<T>
    {
        mockRepository.Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }
}
