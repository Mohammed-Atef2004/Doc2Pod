using Domain.Interfaces.Services;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // =====================
    // User Management
    // =====================
    //public async Task<Result<string>> CreateUserAsync(string email,string userName, string password, CancellationToken ct = default)
    //{
    //    var normalizedEmail = email.ToLowerInvariant();
    //    var user = new ApplicationUser
    //    {
    //        UserName = userName,
    //        Email = normalizedEmail
    //    };

    //    var result = await _userManager.CreateAsync(user, password);


    //    if (!result.Succeeded)
    //        return Result<string>.Failure(MapIdentityErrors(result.Errors));
    //    return Result<string>.Success(user.Id);
    //}
    public async Task<Result<string>> CreateUserAsync(string email, string userName, string password, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        // الحل الأضمن: ولدي الـ ID بنفسك قبل الـ Create
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(), // توليد ID يدوي لضمان وجوده
            UserName = userName,
            Email = normalizedEmail
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return Result<string>.Failure(MapIdentityErrors(result.Errors));

        // تأكدي إن القيمة دي هي اللي بترجع
        return Result<string>.Success(user.Id);
    }

    public async Task<Result> DeleteUserAsync(string identityId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapIdentityErrors(result.Errors));
    }

    // =====================
    // Role Management
    // =====================
    public async Task<Result> AssignRoleAsync(string identityId, UserRole role, CancellationToken ct = default)
    {

        var user = await _userManager.FindByIdAsync(identityId);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var roleName = role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
                return Result.Failure(MapIdentityErrors(roleResult.Errors));
        }

        if (await _userManager.IsInRoleAsync(user, roleName)) return Result.Success();

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapIdentityErrors(result.Errors));
    }

    public async Task<Result> RemoveRoleAsync(string identityId, UserRole role, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var roleName = role.ToString();
        if (!await _userManager.IsInRoleAsync(user, roleName)) return Result.Success();

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapIdentityErrors(result.Errors));
    }

    // =====================
    // Password Management
    // =====================
    public async Task<bool> CheckPasswordAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<Result> ChangePasswordAsync(string identityId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapIdentityErrors(result.Errors));
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException($"Identity user with email {email} not found.");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string identityId, string token, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapIdentityErrors(result.Errors));
    }

    // =====================
    // Email Confirmation
    // =====================
    public async Task<bool> IsEmailConfirmedAsync(string identityId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        return user != null && await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string identityId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId)
            ?? throw new InvalidOperationException($"Identity user {identityId} not found.");

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> ConfirmEmailAsync(string identityId, string token, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.SecurityErrors.InvalidEmailToken);
    }

    // =====================
    // Email Change
    // =====================
    public async Task<string> GenerateChangeEmailTokenAsync(string identityId, string newEmail, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId)
            ?? throw new InvalidOperationException($"Identity user {identityId} not found.");

        return await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
    }

    public async Task<Result> ConfirmEmailChangeAsync(string identityId, string newEmail, string token, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
        if (!result.Succeeded) return Result.Failure(UserErrors.SecurityErrors.InvalidEmailToken);

        await _userManager.UpdateAsync(user);
        return Result.Success();
    }

    // =====================
    // Refresh Token Management 
    // =====================
    public async Task UpdateRefreshTokenAsync(string identityId, string refreshToken, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(identityId);
        if (user != null)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

        }
    }
    // =====================
    // Private Helpers
    // =====================
    private static Error MapIdentityErrors(IEnumerable<IdentityError> errors)
    {
        //var combined = string.Join(" | ", errors.Select(e => e.Description));
        var firstErrorDescription = errors.FirstOrDefault()?.Description;
        return new Error("Identity.Error", string.IsNullOrWhiteSpace(firstErrorDescription) ? "An identity error occurred." : firstErrorDescription);
    }

    public async Task UpdateDomainUserIdAsync(string identityId, Guid domainUserId, CancellationToken ct)
    {
        var identityUser = await _userManager.FindByIdAsync(identityId);
        if (identityUser != null)
        {
            identityUser.DomainUserId = domainUserId;
            await _userManager.UpdateAsync(identityUser);
        }
    }
}