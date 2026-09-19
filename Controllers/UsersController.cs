using high_load_api.Models.DTO;
using high_load_api.Models.Filters;
using high_load_api.Types;
using Microsoft.AspNetCore.Mvc;

namespace high_load_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        public async Task<ActionResult<UsersDTO>> GetUsers()
        {
            try
            {
                UsersDTO userData = await _usersService.GetUsers(new UsersFilter());

                return Ok(userData);
            }
            catch (Exception exception)
            {
                return NotFound(exception);
            }
        }
    }
}
