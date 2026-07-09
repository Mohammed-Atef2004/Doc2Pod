using Microsoft.AspNetCore.SignalR;

namespace API.Hubs.RealTime
{
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("domain_user_id")?.Value;
        }
    }
}
