using Application.Features.Users;
using Application.Features.Users.Commands.Authentication.Register;
using Application.Features.Users.Services;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Settings;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Text;

namespace Application.Auth.Commands.Register
{
    public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identity;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ApiSettings _apiSettings;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IIdentityService identity,
            IEmailService emailService,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiOptions)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _identity = identity;
            _emailService = emailService;
            _auditService = auditService;
            _apiSettings = apiOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var emailResult = Email.Create(command.Email);
            if (emailResult.IsFailure)
                return Result<Guid>.Failure(emailResult.Error);

            var usernameResult = Username.Create(command.Username);
            if (usernameResult.IsFailure)
                return Result<Guid>.Failure(usernameResult.Error);

            var isEmailTaken = await _userRepository.ExistsByEmailAsync(emailResult.Value, ct);
            var isUsernameTaken = await _userRepository.ExistsByUsernameAsync(usernameResult.Value, ct);

            var identityResult = await _identity.CreateUserAsync(command.Email, command.Username, command.Password, ct);
            if (identityResult.IsFailure)
            {
                return Result<Guid>.Failure(identityResult.Error);
            }

            var identityId = identityResult.Value;

            var userResult = User.Create(
                identityId,
                emailResult.Value,
                usernameResult.Value,
                command.FirstName,
                command.LastName,
                isEmailTaken,
                isUsernameTaken,
                UserRole.Student);

            if (userResult.IsFailure)
            {
                await _identity.DeleteUserAsync(identityId, ct);
                return Result<Guid>.Failure(userResult.Error);
            }

            var user = userResult.Value;
            await _userRepository.AddAsync(user);
            await _unitOfWork.CompleteAsync(ct);

            await _identity.UpdateDomainUserIdAsync(identityId, user.Id, ct);

            await _auditService.LogAsync(new AuditEntry(
                ActorId: user.Id,
                Action: AuditActions.UserRegistered,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: ipAddress,
                Succeeded: true), ct);

            var token = await _identity.GenerateEmailConfirmationTokenAsync(identityId, ct);
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            string safeToken = WebEncoders.Base64UrlEncode(tokenBytes);
            var confirmationLink = $"{_apiSettings.FrontendUrl}/confirm-email?" +
                       $"userId={user.Id}&" +
                       $"token={safeToken}";
            var emailResult2 = await _emailService.SendConfirmationEmailAsync(
                 user.Email.Value,
                 user.FullName.DisplayName,
                 confirmationLink,
                 ct);

            if (emailResult2.IsFailure)
            {
                return Result<Guid>.Failure(emailResult2.Error);
            }
            return Result<Guid>.Success(user.Id);
        }
    }
}