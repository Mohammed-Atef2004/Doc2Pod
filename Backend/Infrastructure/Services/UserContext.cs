using Application.Interfaces;
using Microsoft.AspNetCore.Http;


namespace Infrastructure.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor
                    .HttpContext?
                    .User?
                    .FindFirst("domain_user_id")
                    ?.Value;

                if (Guid.TryParse(userId, out var parsedId))
                    return parsedId;

                return null;
            }
        }
    }
}
