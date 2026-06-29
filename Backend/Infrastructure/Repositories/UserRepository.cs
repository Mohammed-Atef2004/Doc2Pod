using Domain.Users;
using Domain.Users.ValueObjects;
using Infrastructure.Presistence.Data;
using Infrastructure.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // قارن الأوبجكت مباشرة (u.Email == email) 
        // الـ EF هيستخدم الـ ValueConverter اللي إنت عامله أوتوماتيكياً
        public Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default)
            => _context.Users.AsNoTracking()
                .AnyAsync(u => u.Email == email, ct);

        public Task<bool> ExistsByUsernameAsync(Username username, CancellationToken ct = default)
            => _context.Users.AsNoTracking()
                .AnyAsync(u => u.Username == username, ct);

        public Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default)
            => _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, ct);

        public Task<User?> GetByUsernameAsync(Username username, CancellationToken ct = default)
            => _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username, ct);
    }
}