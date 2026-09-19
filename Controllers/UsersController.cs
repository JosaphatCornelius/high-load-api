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
        public async Task<ActionResult<UsersDTO>> GetUsers([FromQuery] UsersFilter filter, CancellationToken cancellationToken)
        {
            try
            {
                var userData = await _usersService.GetUsers(filter, cancellationToken);

                return Ok(userData);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
            catch (Exception exception)
            {
                return Problem(
                    detail: exception.Message,
                    title: "Internal Server Error"
                 );
            }
        }
    }
}
