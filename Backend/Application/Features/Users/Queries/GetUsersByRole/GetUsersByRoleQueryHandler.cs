//using Application.Features.Users.Queries.GetUserByEmail;
//using Application.Features.Users.Queries.GetUserById;
//using AutoMapper;
//using Domain.SharedKernel;
//using Domain.Users;
//using MediatR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.Features.Users.Queries.GetUsersByRole
//{
//    public sealed class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, Result<List<GetUsersByRoleResponse>>>
//    {
//        private readonly IUserRepository _userRepository;
//        private readonly IMapper _mapper;
//        public GetUsersByRoleQueryHandler(IUserRepository userRepository, IMapper mapper)
//        {
//            _userRepository = userRepository;
//            _mapper = mapper;
//        }
//        public async Task<Result<List<GetUsersByRoleResponse>>> Handle(GetUsersByRoleQuery query, CancellationToken ct)
//        {
//            Enum.TryParse<UserRole>(query.Role, true, out var role);
//            var users = await _userRepository.GetByRoleAsync(role, ct);
//            var response = _mapper.Map<List<GetUsersByRoleResponse>>(users);
//            return Result<List<GetUsersByRoleResponse>>.Success(response);
//        }
//    }
//}
