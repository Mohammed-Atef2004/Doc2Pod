using Domain.Interfaces.Services;
using Infrastructure.Identity;
using Infrastructure.Presistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EducationalPlatform.Infrastructure.Services.Token;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;
}

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(IOptions<JwtOptions> options, AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _options = options.Value;
        _db = db;
        _userManager = userManager;
    }

    public string GenerateAccessToken(TokenClaims claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtClaims = new[]
        {
            // sub => IdentityId (string) — بيتستخدم في Logout و Token ops
            new Claim(JwtRegisteredClaimNames.Sub,   claims.UserId),
            new Claim(ClaimTypes.NameIdentifier, claims.DomainUserId.ToString()),
            new Claim("domain_user_id",              claims.DomainUserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, claims.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim("username",                    claims.Username),
            new Claim(ClaimTypes.Role,               claims.Role),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: jwtClaims,
            expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return GenerateSecureToken();
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    public async Task<TokenPair?> RotateRefreshTokenAsync(string refreshTokenValue, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenValue, ct);

        if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        var newTokenValue = GenerateSecureToken();

        // جيب الـ Role الحقيقي من الداتابيز بدل الـ hardcoded "User"
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var claims = new TokenClaims(
            UserId: user.Id,                  // IdentityId (string) — في الـ sub
            DomainUserId: user.DomainUserId,  // Guid — في domain_user_id
            Email: user.Email!,
            Username: user.UserName!,
            Role: role);

        user.RefreshToken = newTokenValue;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_options.RefreshTokenDays);

        await _userManager.UpdateAsync(user);

        return new TokenPair(
            AccessToken: GenerateAccessToken(claims),
            RefreshToken: newTokenValue,
            AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            RefreshTokenExpiresAt: user.RefreshTokenExpiryTime);
    }

    public async Task RevokeRefreshTokenAsync(string userId, CancellationToken ct = default)
    {
        var identityUser = await _userManager.FindByIdAsync(userId);

        if (identityUser is null) return;

        identityUser.RefreshToken = null;
        identityUser.RefreshTokenExpiryTime = null;

        var result = await _userManager.UpdateAsync(identityUser);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to revoke refresh token.");
        }
    }

    private static string GenerateSecureToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}