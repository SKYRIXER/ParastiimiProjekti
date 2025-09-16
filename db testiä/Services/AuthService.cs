using db_testiä.Models;
using Microsoft.Extensions.Logging;

namespace db_testiä.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserService userService, ILogger<AuthService> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<LoginResult> LoginAsync(string? name, string? password)
        {
            var trimmedName = name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedName) || string.IsNullOrEmpty(password))
            {
                return LoginResult.InvalidCredentials();
            }

            try
            {
                var user = await _userService.GetByNameAsync(trimmedName);
                if (user is null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    return LoginResult.InvalidCredentials();
                }

                if (!IsPasswordValid(user.PasswordHash, password))
                {
                    return LoginResult.InvalidCredentials();
                }

                if (string.IsNullOrWhiteSpace(user.Id))
                {
                    return LoginResult.MissingIdentifier();
                }

                var response = new LoginResponse
                {
                    UserId = user.Id!,
                    Name = user.Name
                };

                return LoginResult.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate user {UserName}.", trimmedName);
                return LoginResult.Unexpected("An unexpected error occurred while signing in. Please try again.");
            }
        }

        public async Task<CreateUserResult> CreateUserAsync(UserCreateDto input)
        {
            var name = input.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                return CreateUserResult.Validation("Name is required.");
            }

            if (input.Age <= 0)
            {
                return CreateUserResult.Validation("Please provide a valid age greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(input.Password))
            {
                return CreateUserResult.Validation("Password is required.");
            }

            try
            {
                var existing = await _userService.GetByNameAsync(name);
                if (existing is not null)
                {
                    return CreateUserResult.Duplicate("A user with this name already exists.");
                }

                var user = new User
                {
                    Name = name,
                    Age = input.Age,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password)
                };

                await _userService.CreateAsync(user);

                return CreateUserResult.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user {UserName}.", name);
                return CreateUserResult.Failure("An unexpected error occurred while creating the user. Please try again.");
            }
        }

        private bool IsPasswordValid(string storedHash, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, storedHash);
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                _logger.LogWarning(ex, "Stored password hash has an unexpected format. Rejecting login attempt.");
                return false;
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Stored password hash has an invalid format. Rejecting login attempt.");
                return false;
            }
        }
    }

    public sealed class LoginResult
    {
        public bool Succeeded { get; init; }

        public LoginFailureReason Failure { get; init; }

        public string? ErrorMessage { get; init; }

        public LoginResponse? Response { get; init; }

        public static LoginResult Success(LoginResponse response) => new()
        {
            Succeeded = true,
            Failure = LoginFailureReason.None,
            Response = response
        };

        public static LoginResult InvalidCredentials() => new()
        {
            Succeeded = false,
            Failure = LoginFailureReason.InvalidCredentials,
            ErrorMessage = "Invalid username or password."
        };

        public static LoginResult MissingIdentifier() => new()
        {
            Succeeded = false,
            Failure = LoginFailureReason.MissingIdentifier,
            ErrorMessage = "The user does not have a valid identifier."
        };

        public static LoginResult Unexpected(string message) => new()
        {
            Succeeded = false,
            Failure = LoginFailureReason.UnexpectedError,
            ErrorMessage = message
        };
    }

    public enum LoginFailureReason
    {
        None,
        InvalidCredentials,
        MissingIdentifier,
        UnexpectedError
    }

    public sealed class CreateUserResult
    {
        public CreateUserStatus Status { get; init; }

        public string? ErrorMessage { get; init; }

        public User? User { get; init; }

        public bool Succeeded => Status == CreateUserStatus.Success;

        public static CreateUserResult Success(User user) => new()
        {
            Status = CreateUserStatus.Success,
            User = user
        };

        public static CreateUserResult Validation(string message) => new()
        {
            Status = CreateUserStatus.ValidationFailed,
            ErrorMessage = message
        };

        public static CreateUserResult Duplicate(string message) => new()
        {
            Status = CreateUserStatus.DuplicateName,
            ErrorMessage = message
        };

        public static CreateUserResult Failure(string message) => new()
        {
            Status = CreateUserStatus.Failed,
            ErrorMessage = message
        };
    }

    public enum CreateUserStatus
    {
        Success,
        ValidationFailed,
        DuplicateName,
        Failed
    }
}
