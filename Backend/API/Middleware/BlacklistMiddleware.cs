using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;

namespace WebApi.Middlewares;

public sealed class BlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public BlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService blacklist)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (authHeader is not null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var jti = jwtToken.Id; 

                if (!string.IsNullOrEmpty(jti) && blacklist.IsRevoked(jti))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        code = "Auth.TokenRevoked",
                        message = "This token has been revoked. Please login again."
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}