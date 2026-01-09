namespace EventService.Api.Controllers.V1;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Services;
using Models;
using EventService.Data.Exceptions;

[ApiController]
[ApiVersion("1.0")]
[Route("/v{version:apiVersion}/events")]
public class EventController(ILogger<EventController> logger, HandleEventService eventService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(query) && query.Length > 500)
        {
            return BadRequest(new { message = "Search query cannot exceed 500 characters." });
        }

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
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Event ID is required." });
        }

        try
        {
            var result = await eventService.GetById(id, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (FormatException ex)
        {
            return BadRequest(new { message = ex.Message });
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
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Event ID is required." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data.", errors = ModelState });
        }

        try
        {
            var updated = await eventService.Update(id, updateDto, cancellationToken);
            return Ok(new EventDto(updated));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
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
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Event ID is required." });
        }

        try
        {
            var deleted = await eventService.Delete(id, cancellationToken);
            return deleted ? NoContent() : NotFound(new { message = $"Event with ID '{id}' was not found." });
        }
        catch (FormatException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting event with ID: {EventId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the delete request." });
        }
    }
}
