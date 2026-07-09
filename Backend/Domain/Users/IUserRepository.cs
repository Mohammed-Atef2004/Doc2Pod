using Domain.Interfaces.Repositories;
using  Domain.Users.ValueObjects;

namespace  Domain.Users;


public interface IUserRepository:IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(Username username, CancellationToken ct = default);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(Username username, CancellationToken ct = default);


    
}  

