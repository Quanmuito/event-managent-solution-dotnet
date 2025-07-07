using Microsoft.AspNetCore.Mvc;
using EventManagementSolution.Api.Event.Dtos;
using EventManagementSolution.Api.Services;

namespace EventManagementSolution.Api.Event.V1;

[ApiController]
[Route("v1/[controller]")]
public class EventController(ILogger<EventController> logger, MongoDbService mongoService) : ControllerBase
{
    private readonly EventService _eventService = new(mongoService);

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query)
    {
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
        if (result.DeletedCount == 0)
            return NotFound();

        return NoContent();
    }
}