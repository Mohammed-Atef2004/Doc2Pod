using  Domain.SharedKernel;
using  Domain.Users.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Domain.Users.Errors
{
    public static class EmailErrors
    {
        public static readonly Error Empty =
            new("User.Email.Empty", "Email address cannot be empty.");

        public static readonly Error TooLong =
            new("User.Email.TooLong", "Email address must not exceed 254 characters.");

        public static readonly Error InvalidFormat =
            new("User.Email.InvalidFormat", "Email address format is invalid.");
        public static readonly Error SendFailed =
            new("Email.SendFailed", "An error occurred while sending the email. Please try again later.");

        public static readonly Error RemoteServiceError =
            new("Email.RemoteServiceError", "The email service is currently unavailable.");
    }
}
