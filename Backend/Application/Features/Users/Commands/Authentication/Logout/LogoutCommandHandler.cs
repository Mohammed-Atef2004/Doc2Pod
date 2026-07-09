using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Features.Users.Commands.Authentication.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ITokenService _tokenService;
    private readonly ITokenBlacklistService _blacklist;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(
        ITokenService tokenService,
        ITokenBlacklistService blacklist,
        IHttpContextAccessor httpContextAccessor)
    {
        _tokenService = tokenService;
        _blacklist = blacklist;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        // بنجيب الـ IdentityId من الـ sub claim عشان نمسح الـ RefreshToken
        var identityId = user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? user?.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(identityId))
            return Result.Failure(new Error("Auth.Error", "User Identity not found in Security Context."));

        // بنجيب الـ JTI عشان نضيفه للـ Blacklist
        var jti = user?.FindFirstValue(JwtRegisteredClaimNames.Jti);

        // بنجيب وقت انتهاء التوكن من الـ exp claim
        var expClaim = user?.FindFirstValue(JwtRegisteredClaimNames.Exp);
        var tokenExpiry = expClaim is not null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime
            : DateTime.UtcNow.AddMinutes(15); // fallback لو مش موجود

        // 1. نبطل الـ AccessToken فوراً عن طريق الـ Blacklist
        if (!string.IsNullOrEmpty(jti))
            _blacklist.Revoke(jti, tokenExpiry);

        // 2. نمسح الـ RefreshToken من الداتابيز
        await _tokenService.RevokeRefreshTokenAsync(identityId, ct);

        return Result.Success();
    }
}