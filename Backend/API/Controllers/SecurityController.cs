using Application.Features.Users.Commands.Security.Confirm2FASetup;
using Application.Features.Users.Commands.Security.Disable2FA;
using Application.Features.Users.Commands.Security.Setup2FA;
using MassTransit.Mediator;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EducationalPlatform.Presentation.Controllers;

[ApiController]
[Route("api/security")]
[Authorize]
public sealed class SecurityController : ControllerBase
{
    private readonly ISender _sender;

    public SecurityController(ISender sender) => _sender = sender;

    private Guid CurrentUserId =>
     Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) 
     ?? throw new UnauthorizedAccessException("User ID not found in claims"));

    [HttpGet("setup-2fa")]
    public async Task<IActionResult> SetupTwoFactor(CancellationToken ct)
    {
        var result = await _sender.Send(new Setup2FACommand(CurrentUserId), ct);
        if (result.IsSuccess)
            return Ok(result);
        return BadRequest(result.Error);
    }
    [HttpPost("confirm-2fa-setup")]
    public async Task<IActionResult> ConfirmTwoFactorSetup([FromBody] Confirm2FASetupRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new Confirm2FASetupCommand(
            CurrentUserId,
            request.TotpCode,
            request.NewSecret), ct);
        if (result.IsSuccess)
            return Ok(result);
        return BadRequest(result.Error);
    }

    [HttpPost("2fa/disable")]
    public async Task<IActionResult> Disable2FA(CancellationToken ct)
    {
        var result = await _sender.Send(new Disable2FACommand(CurrentUserId), ct);
        if (result.IsSuccess)
            return NoContent();
        return BadRequest(result.Error);
    }
}
public record Confirm2FASetupRequest(string TotpCode, string NewSecret);