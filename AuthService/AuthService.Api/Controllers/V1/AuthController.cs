namespace AuthService.Api.Controllers.V1;

using AuthService.Api.Models;
using AuthService.Api.Services;
using Ems.Common.Http.Responses.Errors;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[ApiVersion("1.0")]
[Route("/v{version:apiVersion}/auths")]
public class AuthController(HandleAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var result = await authService.Register(registerDto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var token = await authService.Login(loginDto, cancellationToken);
        return Ok(token);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var result = await authService.GetById(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuthDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var created = await authService.Create(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new AuthDto(created));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAuthDto updateDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        if (!ModelState.IsValid)
            return BadRequest(new ModelStateErrorResponse(ModelState));

        var updated = await authService.Update(id, updateDto, cancellationToken);
        return Ok(new AuthDto(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new IdRequiredErrorResponse());

        var deleted = await authService.Delete(id, cancellationToken);
        return deleted ? NoContent() : StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse());
    }
}
