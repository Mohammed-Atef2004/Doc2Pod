
using Domain.Users;
using Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.Users;

public class UserTests
{
    

    private const string ValidIdentityId = "identity-id";
    private const string ValidEmail = "test@test.com";
    private const string ValidUsername = "testuser";

    private User CreateUser(
        bool isEmailTaken = false,
        bool isUsernameTaken = false)
    {
        var email = Email.Create(ValidEmail).Value;
        var username = Username.Create(ValidUsername).Value;

        var result = User.Create(
            ValidIdentityId,
            email,
            username,
            "Mohamed",
            "Atef",
            isEmailTaken,
            isUsernameTaken);

        return result.Value;
    }

   

    [Fact]
    public void Create_Should_Succeed()
    {
        var email = Email.Create(ValidEmail).Value;
        var username = Username.Create(ValidUsername).Value;

        var result = User.Create(
            ValidIdentityId,
            email,
            username,
            "Mohamed",
            "Atef",
            false,
            false);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Value.Should().Be(ValidEmail);
    }

    [Fact]
    public void Create_Should_Fail_When_Email_Taken()
    {
        var email = Email.Create(ValidEmail).Value;
        var username = Username.Create(ValidUsername).Value;

        var result = User.Create(
            ValidIdentityId,
            email,
            username,
            "Mohamed",
            "Atef",
            true,
            false);

        result.IsFailure.Should().BeTrue();
    }

 

    [Fact]
    public void ChangeName_Should_Update_Name()
    {
        var user = CreateUser();

        var result = user.ChangeName("Ali", "Hassan");

        result.IsSuccess.Should().BeTrue();
        user.FullName.DisplayName.Should().Be("Ali Hassan");
    }

    [Fact]
    public void UpdateEmail_Should_Work()
    {
        var user = CreateUser();
        var newEmail = Email.Create("new@test.com").Value;

        var result = user.UpdateEmail(newEmail, false);

        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be("new@test.com");
    }

   

    [Fact]
    public void RecordPasswordChange_Should_Add_To_History()
    {
        var user = CreateUser();

        var result = user.RecordPasswordChange("hash1", false);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHashes.Should().Contain("hash1");
    }

    [Fact]
    public void RecordPasswordChange_Should_Fail_When_Reused()
    {
        var user = CreateUser();

        var result = user.RecordPasswordChange("hash1", true);

        result.IsFailure.Should().BeTrue();
    }

   

    [Fact]
    public void RecordFailedLogin_Should_Lock_User()
    {
        var user = CreateUser();

        for (int i = 0; i < 10; i++)
            user.RecordFailedLogin("127.0.0.1");

        user.LockedUntil.Should().NotBeNull();
    }

    [Fact]
    public void UnlockAccount_Should_Reset_State()
    {
        var user = CreateUser();

        for (int i = 0; i < 10; i++)
            user.RecordFailedLogin("127.0.0.1");

        user.UnlockAccount();

        user.LockedUntil.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
    }

    

    [Fact]
    public void ConfirmEmail_Should_Set_Flag()
    {
        var user = CreateUser();

        var result = user.ConfirmEmail();

        result.IsSuccess.Should().BeTrue();
        user.IsEmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void Delete_Should_SoftDelete()
    {
        var user = CreateUser();

        var result = user.Delete("test");

        result.IsSuccess.Should().BeTrue();
        user.IsDeleted.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void EnableTwoFactor_Should_Work()
    {
        var user = CreateUser();

        var result = user.EnableTwoFactor("secret");

        result.IsSuccess.Should().BeTrue();
        user.IsTwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public void DisableTwoFactor_Should_Work()
    {
        var user = CreateUser();
        user.EnableTwoFactor("secret");

        var result = user.DisableTwoFactor();

        result.IsSuccess.Should().BeTrue();
        user.IsTwoFactorEnabled.Should().BeFalse();
    }
}