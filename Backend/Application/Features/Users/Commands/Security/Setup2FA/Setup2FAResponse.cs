using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Security.Setup2FA
{
    public record Setup2FAResponse(string Secret, string QrCodeUri);
}
