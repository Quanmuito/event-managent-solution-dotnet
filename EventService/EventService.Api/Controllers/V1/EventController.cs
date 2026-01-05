namespace EventService.Api.Controllers.V1;
using Asp.Versioning;
using Services;
using Microsoft.AspNetCore.Mvc;
using Models.Api.Event;

[ApiController]
[ApiVersion("1.0")]
[Route("/v{version:apiVersion}/events")]
public class EventController(ILogger<EventController> logger, HandleEventService eventService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("Search query: {Query}", query);
            var results = await eventService.Search(query, cancellationToken);
            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while searching events with query: {Query}", query);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the search request." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await eventService.GetById(id, cancellationToken);
            return Ok(result);
        }
        catch (NullReferenceException)
        {
            logger.LogWarning("Event with ID '{EventId}' was not found.", id);
            return NotFound(new { message = $"Event with ID '{id}' was not found." });
        }
        catch (FormatException ex)
        {
            logger.LogWarning(ex, "Invalid event ID format: {EventId}", id);
            return BadRequest(new { message = "Invalid event ID format." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving event with ID: {EventId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the request." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data.", errors = ModelState });
        }

        try
        {
            var created = await eventService.Create(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new EventDto(created));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument while creating event");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating event");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the request." });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateEventDto updateDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data.", errors = ModelState });
        }

        try
        {
            var updated = await eventService.Update(id, updateDto, cancellationToken);
            return Ok(new EventDto(updated));
        }
        catch (NullReferenceException)
        {
            logger.LogWarning("Event with ID '{EventId}' was not found for update.", id);
            return NotFound(new { message = $"Event with ID '{id}' was not found." });
        }
        catch (FormatException ex)
        {
            logger.LogWarning(ex, "Invalid event ID format: {EventId}", id);
            return BadRequest(new { message = "Invalid event ID format." });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument while updating event with ID: {EventId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating event with ID: {EventId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the request." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await eventService.Delete(id, cancellationToken);

            return result.DeletedCount == 0 ? NotFound(new { message = $"Event with ID '{id}' was not found." }) : NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting event with ID: {EventId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the delete request." });
        }
    }
}