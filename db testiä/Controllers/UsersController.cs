using db_testiä.Models;
using db_testiä.Services;
using Microsoft.AspNetCore.Mvc;

namespace db_testiä.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult<List<User>> Get() => _userService.Get();

        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<User> Get(string id)
        {
            var user = _userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public ActionResult<User> Create(UserCreateDto userDto)
        {
            var user = new User
            {
                Name = userDto.Name,
                Age = userDto.Age,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };

            _userService.Create(user);
            return CreatedAtRoute("GetUser", new { id = user.Id }, user);
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var user = _userService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            _userService.Delete(id);

            return NoContent();
        }
    }
}
