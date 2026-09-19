using high_load_api.Models.DTO;
using high_load_api.Models.Filters;
using high_load_api.Types;

namespace high_load_api.Services
{
    public class UsersService : IUsersService
    {
        public async Task<UsersDTO> GetUsers(UsersFilter filter)
        {
            return new UsersDTO() { ID = 1, Email = "test@mail.com", Name = "Test" };
        }
    }
}
