using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface ITokenBlacklistService
    {
        void Revoke(string jti, DateTime tokenExpiry);
        bool IsRevoked(string jti);
    }
}
