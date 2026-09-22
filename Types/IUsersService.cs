using high_load_api.Models.Database;
using high_load_api.Models.DTO;
using high_load_api.Models.Filters;

namespace high_load_api.Types
{
    public interface IUsersService
    {
        public Task<List<UsersDTO>> GetUsers(UsersFilter filter, CancellationToken cancellationToken);
        public Task<UsersModel?> GetUserDetail(long userID, CancellationToken cancellationToken);
    }
}
