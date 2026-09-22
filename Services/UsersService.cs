using high_load_api.Models.Database;
using high_load_api.Models.Database.Context;
using high_load_api.Models.DTO;
using high_load_api.Models.Filters;
using high_load_api.Types;
using Microsoft.EntityFrameworkCore;

namespace high_load_api.Services
{
    public class UsersService : IUsersService
    {
        private readonly ECommerceDBContext _eCommDBContext;

        public UsersService(ECommerceDBContext eCommDBContext)
        {
            _eCommDBContext = eCommDBContext;
        }

        public async Task<List<UsersDTO>> GetUsers(UsersFilter filter, CancellationToken cancellationToken)
        {
            var query = _eCommDBContext.Users.AsNoTracking();

            if (filter.ID.HasValue)
            {
                query = query.Where(u => u.ID == filter.ID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(u => u.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                query = query.Where(u => u.Email == filter.Email);
            }

            var users = await query
                .Select(u => new UsersDTO()
                {
                    ID = u.ID,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToListAsync(cancellationToken);

            return users;
        }

        public async Task<UsersModel?> GetUserDetail(long userID, CancellationToken cancellationToken)
        {
            var user = await _eCommDBContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.ID == userID, cancellationToken);

            return user ?? throw new KeyNotFoundException($"User with ID {userID} is not found");
        }
    }
}
