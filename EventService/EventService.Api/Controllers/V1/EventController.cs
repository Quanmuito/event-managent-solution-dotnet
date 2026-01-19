namespace EventService.Api.Controllers.V1;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Ems.Common.Http.Responses.Errors;
using Services;
using Models;

[ApiController]
[ApiVersion("1.0")]
[Route("/v{version:apiVersion}/events")]
public class EventController(HandleEventService eventService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(query) && query.Length > 500)
            return BadRequest(new QueryLengthErrorResponse());

        var results = await eventService.Search(query, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var result = await eventService.GetById(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var created = await eventService.Create(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new EventDto(created));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateEventDto updateDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var updated = await eventService.Update(id, updateDto, cancellationToken);
        return Ok(new EventDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var deleted = await eventService.Delete(id, cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse());
    }
}
