using high_load_api.Models.DTO;
using high_load_api.Models.Filters;

namespace high_load_api.Types
{
    public interface IUsersService
    {
        public Task<List<UsersDTO>> GetUsers(UsersFilter filter, CancellationToken cancellationToken);
    }
}
