using AutoMapper;
using Domain.SharedKernel;
using Domain.Users;
using Domain.Users.Errors;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUserById
{
    public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<GetUserByIdResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<GetUserByIdResponse>> Handle(GetUserByIdQuery query, CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(query.UserId);
            if (user is null)
                return Result<GetUserByIdResponse>.Failure(UserErrors.NotFound);
            var response = _mapper.Map<GetUserByIdResponse>(user);
            return Result<GetUserByIdResponse>.Success(response);
        }
    }
}
