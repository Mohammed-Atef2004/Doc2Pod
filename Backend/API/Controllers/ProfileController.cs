using Application.Features.Users.Commands.Profile;
using Application.Features.Users.Commands.Profile.ChangeEmail;
using Application.Features.Users.Commands.Profile.ChangePassword;
using Application.Features.Users.Commands.Profile.SetPhoneNumber;
using Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue("domain_user_id")
        ?? throw new UnauthorizedAccessException("User ID not found in claims"));

    [HttpPut("change-name")]
    public async Task<IActionResult> ChangeName(
        [FromBody] ChangeNameRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeNameCommand(
            CurrentUserId,
            request.FirstName,
            request.LastName);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(
        [FromBody] ChangeEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeEmailCommand(
            CurrentUserId,
            request.NewEmail,
            request.CurrentPassword,
            request.TwoFactorCode);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(
            CurrentUserId,
            request.CurrentPassword,
            request.NewPassword,
            request.TwoFactorCode);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result.Error);
    }

    [HttpPut("set-phone-number")]
    public async Task<IActionResult> SetPhoneNumber(
        [FromBody] SetPhoneNumberRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetPhoneNumberCommand(
            CurrentUserId,
            request.NewPhoneNumber,
            request.CurrentPassword,
            request.TwoFactorCode);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetProfileData(CancellationToken ct)
    {
        var query = new GetUserByIdQuery(CurrentUserId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

public sealed record ChangeNameRequest(
    string FirstName,
    string LastName);

public sealed record ChangeEmailRequest(
    string NewEmail,
    string CurrentPassword,
    string? TwoFactorCode);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string? TwoFactorCode);

public sealed record SetPhoneNumberRequest(
    string NewPhoneNumber,
    string CurrentPassword,
    string? TwoFactorCode);