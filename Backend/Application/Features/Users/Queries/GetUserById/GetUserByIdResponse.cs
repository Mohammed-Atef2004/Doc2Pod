using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Queries.GetUserById
{
    public record GetUserByIdResponse(
     Guid Id,
     string Username,
     string Email,
     string firstname,
     string lastname,
     string? phoneNumber,
     string Role,
     bool IsActive,
     bool IsEmailConfirmed,
     bool IsTwoFactorEnabled,
    DateTime? LastLogin);
}
