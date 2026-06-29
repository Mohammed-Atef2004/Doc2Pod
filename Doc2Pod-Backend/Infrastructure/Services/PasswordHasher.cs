using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12;
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
        }
    }
}
