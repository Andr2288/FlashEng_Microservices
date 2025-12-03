using FlashEng.Bll.Dto;
using FlashEng.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace FlashEng.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("User registration attempt: {Email}", registerDto.Email);

                // Конвертуємо RegisterDto в CreateUserDto
                var createUserDto = new CreateUserDto
                {
                    Email = registerDto.Email,
                    FullName = registerDto.Name, // Припускаємо що Name це FullName
                    Password = registerDto.Password,
                    Role = registerDto.Name?.ToLower().Contains("admin") == true ? "Admin" : "User"
                };

                var userId = await _userService.CreateUserAsync(createUserDto, cancellationToken);
                var user = await _userService.GetUserByIdAsync(userId, cancellationToken);

                if (user == null)
                    return BadRequest("Failed to create user");

                // Генеруємо простий токен (в реальному додатку використовуйте JWT)
                var token = GenerateSimpleToken(user);

                var response = new AuthResponseDto
                {
                    Id = user.UserId,
                    Name = user.FullName,
                    Email = user.Email,
                    Token = token,
                    IsAdmin = user.Role?.ToLower() == "admin" || user.FullName?.ToLower().Contains("admin") == true
                };

                _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", registerDto.Email);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("User login attempt: {Email}", loginDto.Email);

                var user = await _userService.GetUserByEmailAsync(loginDto.Email, cancellationToken);

                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                // В реальному додатку тут має бути перевірка хешу пароля
                // Для простоти припускаємо що пароль правильний

                // Генеруємо токен
                var token = GenerateSimpleToken(user);

                var response = new AuthResponseDto
                {
                    Id = user.UserId,
                    Name = user.FullName,
                    Email = user.Email,
                    Token = token,
                    IsAdmin = user.Role?.ToLower() == "admin" || user.FullName?.ToLower().Contains("admin") == true
                };

                _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", loginDto.Email);
                return Unauthorized(new { message = "Login failed" });
            }
        }

        [HttpGet("check")]
        public async Task<ActionResult<AuthResponseDto>> CheckAuth()
        {
            try
            {
                // Отримуємо токен з заголовку Authorization
                var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "No token provided" });

                // Простий спосіб отримання email з токену (в реальному додатку використовуйте JWT)
                var email = DecodeSimpleToken(token);

                if (string.IsNullOrEmpty(email))
                    return Unauthorized(new { message = "Invalid token" });

                var user = await _userService.GetUserByEmailAsync(email);

                if (user == null)
                    return Unauthorized(new { message = "User not found" });

                var response = new AuthResponseDto
                {
                    Id = user.UserId,
                    Name = user.FullName,
                    Email = user.Email,
                    Token = token,
                    IsAdmin = user.Role?.ToLower() == "admin" || user.FullName?.ToLower().Contains("admin") == true
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auth check failed");
                return Unauthorized(new { message = "Authentication failed" });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // В простій реалізації просто повертаємо успіх
            // В реальному додатку можна додати токен до blacklist
            return Ok(new { message = "Logged out successfully" });
        }

        // Простий метод генерації токену (НЕ для продакшн використання)
        private string GenerateSimpleToken(UserDto user)
        {
            var tokenData = $"{user.Email}:{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}:{user.UserId}";
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(tokenData);
            return Convert.ToBase64String(tokenBytes);
        }

        // Простий метод декодування токену
        private string DecodeSimpleToken(string token)
        {
            try
            {
                var tokenBytes = Convert.FromBase64String(token);
                var tokenData = System.Text.Encoding.UTF8.GetString(tokenBytes);
                return tokenData.Split(':')[0]; // Повертаємо email
            }
            catch
            {
                return null;
            }
        }
    }

    // DTOs для авторизації
    public class RegisterDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Phone { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Token { get; set; } = "";
        public bool IsAdmin { get; set; }
    }
}
