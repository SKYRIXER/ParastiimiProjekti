using db_testiä.Models;
using db_testiä.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace db_testiä.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public UsersController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> Get()
        {
            var users = await _userService.GetAsync();
            return users;
        }

        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var user = await _userService.GetAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(UserCreateDto userDto)
        {
            var result = await _authService.CreateUserAsync(userDto);
            if (result.Status == CreateUserStatus.Success && result.User is not null)
            {
                return CreatedAtRoute("GetUser", new { id = result.User.Id }, result.User);
            }

            return result.Status switch
            {
                CreateUserStatus.ValidationFailed => BadRequest(new { message = result.ErrorMessage }),
                CreateUserStatus.DuplicateName => Conflict(new { message = result.ErrorMessage }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = result.ErrorMessage ?? "Failed to create user." })
            };
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _userService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
