using Application.Features.Users.Services;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Security.Setup2FA
{
    public sealed class Setup2FACommandHandler
           : IRequestHandler<Setup2FACommand, Result<Setup2FAResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;

        public Setup2FACommandHandler(IUserRepository userRepository, ITotpService totpService)
        {
            _userRepository = userRepository;
            _totpService = totpService;
        }

        public async Task<Result<Setup2FAResponse>> Handle(Setup2FACommand command, CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result<Setup2FAResponse>.Failure(UserErrors.NotFound);

            var secret = _totpService.GenerateSecret();

            var qrCodeUri = _totpService.GenerateQrCodeUri(user.Email.Value, secret);

            return Result<Setup2FAResponse>.Success(new Setup2FAResponse(secret, qrCodeUri));
        }
    }

}
