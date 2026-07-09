using Domain.SharedKernel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Profile
{
    public sealed record ChangeNameCommand(
        Guid UserId,    
        string FirstName, 
        string LastName
    ) : IRequest<Result>;
}
