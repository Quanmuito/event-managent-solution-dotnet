namespace BookingService.Api.Controllers.V1;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using DatabaseService.Exceptions;
using Services;
using Models;

[ApiController]
[ApiVersion("1.0")]
[Route("/v{version:apiVersion}/bookings")]
public class BookingController(ILogger<BookingController> logger, HandleBookingService bookingService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Booking ID is required." });
        }

        try
        {
            var result = await bookingService.GetById(id, cancellationToken);
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
            logger.LogError(ex, "An error occurred while retrieving booking with ID: {BookingId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the request." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data.", errors = ModelState });
        }

        try
        {
            var created = await bookingService.Create(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new BookingDto(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating booking");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the request." });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBookingDto updateDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Booking ID is required." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request data.", errors = ModelState });
        }

        try
        {
            var updated = await bookingService.Update(id, updateDto, cancellationToken);
            return Ok(new BookingDto(updated));
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
            logger.LogError(ex, "An error occurred while updating booking with ID: {BookingId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the request." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Booking ID is required." });
        }

        try
        {
            var deleted = await bookingService.Delete(id, cancellationToken);
            return deleted ? NoContent() : NotFound(new { message = $"Booking with ID '{id}' was not found." });
        }
        catch (FormatException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting booking with ID: {BookingId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing the delete request." });
        }
    }
}
