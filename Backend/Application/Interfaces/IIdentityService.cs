using Domain.SharedKernel;
using Domain.Users;

namespace Domain.Interfaces.Services
{
    public interface IIdentityService
    {
        // =====================
        // User Management
        // =====================
        Task<Result<string>> CreateUserAsync(string email, string userName, string password, CancellationToken ct = default);
        Task<Result> DeleteUserAsync(string identityId, CancellationToken ct = default);

        // =====================
        // Role Management
        // =====================
        Task<Result> AssignRoleAsync(string identityId, UserRole role, CancellationToken ct = default);
        Task<Result> RemoveRoleAsync(string identityId, UserRole role, CancellationToken ct = default);

        // =====================
        // Password Management
        // =====================
        Task<bool> CheckPasswordAsync(string email, string password, CancellationToken ct = default);
        Task<Result> ChangePasswordAsync(string identityId, string currentPassword, string newPassword, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(string identityId, string token, string newPassword, CancellationToken ct = default);
        Task<string> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default);

        // =====================
        // Email Confirmation
        // =====================
        Task<bool> IsEmailConfirmedAsync(string identityId, CancellationToken ct = default);
        Task<string> GenerateEmailConfirmationTokenAsync(string identityId, CancellationToken ct = default);
        Task<Result> ConfirmEmailAsync(string identityId, string token, CancellationToken ct = default);

        // =====================
        // Email Change
        // =====================
        Task<string> GenerateChangeEmailTokenAsync(string identityId, string newEmail, CancellationToken ct = default);
        Task<Result> ConfirmEmailChangeAsync(string identityId, string newEmail, string token, CancellationToken ct = default);

        Task UpdateRefreshTokenAsync(string identityId, string refreshToken, CancellationToken ct = default);

        Task UpdateDomainUserIdAsync(string identityId, Guid domainUserId, CancellationToken ct);
    }
}