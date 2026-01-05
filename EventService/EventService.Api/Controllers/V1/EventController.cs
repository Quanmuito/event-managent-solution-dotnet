namespace EventService.Api.Controllers.V1;

using Data;
using Services;
using Microsoft.AspNetCore.Mvc;
using Models.Api.Event;

[ApiController]
[Route("/v{version:apiVersion}/events")]
//TODO: Controller should use only services (e.g. business logic), and not any db context,
// then services should use db context or repositories to access data
public class EventController(ILogger<EventController> logger, MongoDbContext mongoService) : ControllerBase
{
    private readonly EventService _eventService = new(mongoService);

    //TODO: Requests should support cancellation tokens, e.g. if search is taking much time,
    // you might want to cancel it without blocking resources to finish the request
    //TODO: YOu don't need a separate path for it, you just can use [HttpGet] but with a filter query
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query)
    {
        logger.LogDebug("Search query: {Query}", query);
        var results = await _eventService.Search(query);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _eventService.GetById(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventDto createDto)
    {
        try
        {
            var created = await _eventService.Create(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new EventDto(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateEventDto updateDto)
    {
        try
        {
            var updated = await _eventService.Update(id, updateDto);
            return Ok(new EventDto(updated));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _eventService.Delete(id);

        //TODO: This is should be ok, but DeleteCount property can throw an exception if the operation is unacknowledged
        // so you should handle it properly. I would suggest to wrap ALL endpoints with a try-catch block with proper logging
        // and if an exception occurs, return a 500 Internal Server Error and log the exception
        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }
}