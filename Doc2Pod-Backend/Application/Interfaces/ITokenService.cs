using System.Security.Claims;

namespace Domain.Interfaces.Services;

public sealed record TokenClaims(
    string UserId,        
    Guid DomainUserId,    
    string Email,
    string Username,
    string Role);

public sealed record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime? RefreshTokenExpiresAt);

public interface ITokenService
{
    string GenerateAccessToken(TokenClaims claims);

    string GenerateRefreshToken();

    Task<TokenPair?> RotateRefreshTokenAsync(string refreshTokenValue, CancellationToken ct = default);

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

    Task RevokeRefreshTokenAsync(string userId, CancellationToken ct = default);
}