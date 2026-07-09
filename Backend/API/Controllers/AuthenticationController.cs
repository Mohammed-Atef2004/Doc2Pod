using Application.Features.Users.Commands.Authentication.ConfirmEmail;
using Application.Features.Users.Commands.Authentication.ConfirmEmailChange;
using Application.Features.Users.Commands.Authentication.ForgotPassword;
using Application.Features.Users.Commands.Authentication.Login;
using Application.Features.Users.Commands.Authentication.Logout;
using Application.Features.Users.Commands.Authentication.Register;
using Application.Features.Users.Commands.Authentication.ResetPassword;
using Application.Features.Users.Commands.Authentication.VerifyTwoFactor;
using Application.Features.Users.Commands.Authentication.VerifyTwoFactorLogin;
using Domain.Users.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        // بنقرأ من "domain_user_id" اللي حطيناه في التوكن بدل sub
        // لأن sub فيه IdentityId (string) مش الـ Domain Guid
        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue("domain_user_id")
            ?? throw new UnauthorizedAccessException("User ID not found in claims"));

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // ─────────────────────────────────────────────
        // Authentication
        // ─────────────────────────────────────────────

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Error);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (result.IsFailure)
                return BadRequest(result.Error);
            return Ok(result);
        }


        [HttpPost("verify-2fa-login")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyTwoFactorLogin([FromBody] Verify2FALoginRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new VerifyTwoFactorLoginCommand(request.UserId, request.TotpCode), ct);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var result = await _mediator.Send(new LogoutCommand());
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken ct)
        {
            byte[] decodedBytes = WebEncoders.Base64UrlDecode(token);
            string originalToken = Encoding.UTF8.GetString(decodedBytes);

            var command = new ConfirmEmailCommand(userId, originalToken);
            var result = await _mediator.Send(command, ct);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("confirm-email-change")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailChange(
            [FromQuery] Guid userId,
            [FromQuery] string newEmail,
            [FromQuery] string token,
            CancellationToken ct)
        {
            byte[] decodedBytes = WebEncoders.Base64UrlDecode(token);
            string originalToken = Encoding.UTF8.GetString(decodedBytes);

            var result = await _mediator.Send(new ConfirmEmailChangeCommand(userId, newEmail, originalToken), ct);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Error);
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Error);
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
        {
            byte[] decodedBytes = WebEncoders.Base64UrlDecode(command.Token);
            string originalToken = Encoding.UTF8.GetString(decodedBytes);
            var updatedCommand = command with { Token = originalToken };
            var result = await _mediator.Send(updatedCommand, ct);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
    }

        public record Verify2FALoginRequest(Guid UserId, string TotpCode);
    }

