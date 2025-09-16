using db_testiä.Models;
using db_testiä.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace db_testiä.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.LoginAsync(request.Name, request.Password);
            if (result.Succeeded && result.Response is not null)
            {
                return Ok(result.Response);
            }

            return result.Failure switch
            {
                LoginFailureReason.MissingIdentifier => BadRequest(new { message = result.ErrorMessage ?? "The user does not have a valid identifier." }),
                LoginFailureReason.InvalidCredentials => Unauthorized(new { message = result.ErrorMessage ?? "Invalid username or password." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = result.ErrorMessage ?? "An unexpected error occurred while signing in." })
            };
        }
    }
}
