using high_load_api.Models.Database;
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
        public async Task<ActionResult<List<UsersDTO>>> GetUsers([FromQuery] UsersFilter filter, CancellationToken cancellationToken)
        {
            var userData = await _usersService.GetUsers(filter, cancellationToken);

            return Ok(userData);
        }

        [HttpGet("{userID}")]
        public async Task<ActionResult<UsersModel>> GetUserDetail([FromRoute] long userID, CancellationToken cancellationToken)
        {
            var userData = await _usersService.GetUserDetail(userID, cancellationToken);

            return Ok(userData);
        }
    }
}
