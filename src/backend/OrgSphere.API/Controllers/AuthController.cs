using Microsoft.AspNetCore.Mvc;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;

namespace OrgSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var result = await authService.LoginAsync(request.Email, request.Password, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var result = await authService.RegisterAsync(
                request.Email, request.Password, request.FirstName, request.LastName, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        try
        {
            var result = await authService.RefreshTokenAsync(request.RefreshToken, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await authService.RevokeTokenAsync(request.RefreshToken, ct);
        return Ok(new { message = "Token revoked" });
    }
}
