using Domain.SharedKernel;

namespace Domain.Users.Errors;

public static class UserErrors
{
    public static readonly Error NotFound =
        new("User.NotFound", "The requested user does not exist.");

    //public static readonly Error AlreadyExists =
    //    new("User.AlreadyExists", "A user with this email address already exists.");

    public static readonly Error SameEmail =
    new("User.SameEmail", "The new email address cannot be the same as the current one.");

    public static readonly Error Deactivated =
        new("User.Deactivated", "This user account has been deactivated.");

    public static readonly Error AlreadyDeleted =
        new("User.AlreadyDeleted", "This user account has already been deleted.");

    public static readonly Error AlreadyActive =
        new("User.AlreadyActive", "This user account is already active.");

    public static readonly Error InvalidIdentityReference =
        new("User.InvalidIdentityReference", "The provided identity ID is null or empty.");

    public static readonly Error CannotDeleteSuperAdmin =
        new("User.CannotDeleteSuperAdmin", "SuperAdmin accounts cannot be deleted.");

    public static readonly Error CannotDeactivateSuperAdmin =
        new("User.CannotDeactivateSuperAdmin", "SuperAdmin accounts cannot be deactivated.");

    public static readonly Error RoleAlreadyAssigned =
        new("User.RoleAlreadyAssigned", "The user already has this role.");

    public static readonly Error CannotAssignSuperAdminRole =
        new("User.CannotAssignSuperAdminRole", "SuperAdmin role cannot be assigned through normal operations.");

    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Authentication failed. Please check your credentials.");

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Please confirm your email address before logging in.");

    public static readonly Error AccountDeleted =
        new("User.AccountDeleted", "This account has been deleted and cannot be accessed.");

    public static readonly Error InvalidUserId =
    new(
        "User.InvalidUserId",
        "The current user's ID is missing or invalid."
    );



    public static class SecurityErrors
    {
        public static readonly Error AccountLocked =
            new("User.Security.AccountLocked",
                "Account is temporarily locked due to too many failed login attempts.");

        public static readonly Error TwoFactorAlreadyEnabled =
            new("User.Security.TwoFactorAlreadyEnabled", "Two-factor authentication is already enabled.");

        public static readonly Error TwoFactorNotEnabled =
            new("User.Security.TwoFactorNotEnabled", "Two-factor authentication is not enabled.");

        public static readonly Error InvalidTwoFactorSecret =
            new("User.Security.InvalidTwoFactorSecret", "Two-factor secret cannot be empty.");

        public static readonly Error PasswordAlreadyUsed =
            new("User.Security.PasswordAlreadyUsed",
                "This password was used recently. Please choose a different password.");
        public static readonly Error InvalidEmailToken =
            new("User.Security.InvalidEmailToken", "The provided email token is invalid or expired.");
        public static readonly Error InvalidTotpCode =
            new("User.Security.InvalidTotpCode", "The provided TotpCode is Invalid");
        public static readonly Error TokenGenerationFailed =
            new("User.Security.TokenGenerationFailed", "An error occurred while generating the security token. Please try again.");
    }

}