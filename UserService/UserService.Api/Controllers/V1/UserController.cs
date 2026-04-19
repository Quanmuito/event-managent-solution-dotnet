namespace UserService.Api.Controllers.V1;

using Asp.Versioning;
using Ems.Common.Http.Responses.Errors;
using Microsoft.AspNetCore.Mvc;
using UserService.Api.Models;
using UserService.Api.Services;

[ApiController]
[ApiVersion("1.0")]
[Route("/v{version:apiVersion}/users")]
public class UserController(HandleUserService userService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(query) && query.Length > 500)
            return BadRequest(new QueryLengthErrorResponse());

        var results = await userService.Search(query, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var result = await userService.GetById(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var created = await userService.Create(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new UserDto(created));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var updated = await userService.Update(id, updateDto, cancellationToken);
        return Ok(new UserDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var deleted = await userService.Delete(id, cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse());
    }
}
