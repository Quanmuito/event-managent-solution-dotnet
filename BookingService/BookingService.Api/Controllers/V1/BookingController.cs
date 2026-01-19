namespace BookingService.Api.Controllers.V1;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Ems.Common.Http.Responses.Errors;
using Services;
using Models;

[ApiController]
[ApiVersion("1.0")]
[Route("/v{version:apiVersion}/bookings")]
public class BookingController(HandleBookingService bookingService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var result = await bookingService.GetById(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var created = await bookingService.Create(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new BookingDto(created));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBookingDto updateDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var updated = await bookingService.Update(id, updateDto, cancellationToken);
        return Ok(new BookingDto(updated));
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var confirmed = await bookingService.Confirm(id, cancellationToken);
        return Ok(new BookingDto(confirmed));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var canceled = await bookingService.Cancel(id, cancellationToken);
        return Ok(new BookingDto(canceled));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var deleted = await bookingService.Delete(id, cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse());
    }
}
