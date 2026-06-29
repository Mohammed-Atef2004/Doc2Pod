using Application.Features.Users.Queries.GetUserById;
using AutoMapper;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using Domain.Users.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUserByEmail
{
    public sealed class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<GetUserByEmailResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByEmailQueryHandler(IUserRepository userRepository, IMapper mapper) 
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetUserByEmailResponse>> Handle(GetUserByEmailQuery query, CancellationToken ct)
        {
            var emailResult = Email.Create(query.Email);
            if (emailResult.IsFailure)
                return Result<GetUserByEmailResponse>.Failure(emailResult.Error);

            var user = await _userRepository.GetByEmailAsync(emailResult.Value, ct);

            if (user is null)
                return Result<GetUserByEmailResponse>.Failure(UserErrors.NotFound);
            var response = _mapper.Map<GetUserByEmailResponse>(user);
            return Result<GetUserByEmailResponse>.Success(response);
        }
    }
}
