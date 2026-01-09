namespace EventService.Data.Exceptions;

public class NotFoundException(string collectionName, string id) : Exception($"{collectionName} with ID '{id}' was not found.")
{
}
