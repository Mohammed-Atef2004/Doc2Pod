using Application.Features.Users.Queries.GetUserById;
using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUsersByRole
{
    public record GetUsersByRoleQuery(string Role) : IRequest<Result<List<GetUsersByRoleResponse>>>;
}
