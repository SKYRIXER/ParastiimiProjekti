using db_testiä.Models;
using db_testiä.Services;
using Microsoft.AspNetCore.Mvc;

namespace db_testiä.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = _userService.GetByName(request.Name);
            if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (string.IsNullOrEmpty(user.Id))
            {
                return BadRequest(new { message = "The user does not have a valid identifier." });
            }

            var response = new LoginResponse
            {
                UserId = user.Id,
                Name = user.Name
            };

            return Ok(response);
        }
    }
}
