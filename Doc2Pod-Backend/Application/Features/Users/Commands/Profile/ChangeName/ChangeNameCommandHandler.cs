using Application.Features.Users.Services;
using Domain.Interfaces.Repositories;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Users.Errors.UserErrors;

namespace Application.Features.Users.Commands.Profile.ChangeName
{
    public sealed class ChangeNameCommandHandler : IRequestHandler<ChangeNameCommand, Result>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ChangeNameCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _auditService = auditService;
        }

        public async Task<Result> Handle(ChangeNameCommand command, CancellationToken ct)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user is null)
                return Result.Failure(UserErrors.NotFound);

            var availabilityResult = user.CheckAvailability();
            if (availabilityResult.IsFailure)
                return availabilityResult;

            if (user.FullName.FirstName == command.FirstName && user.FullName.LastName == command.LastName)
                return Result.Success();

            var result = user.ChangeName(command.FirstName, command.LastName);
            if (result.IsFailure)
                return result;

            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync(ct);

            await _auditService.LogAsync(new AuditEntry(
                ActorId: user.Id,
                Action: AuditActions.UserChangeName,
                EntityType: nameof(User),
                EntityId: user.Id,
                IpAddress: ipAddress,
                Succeeded: true), ct);

            return Result.Success();
        }
    }
}
