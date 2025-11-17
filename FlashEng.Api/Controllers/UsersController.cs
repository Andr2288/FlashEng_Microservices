using FlashEng.Bll.Dto;
using FlashEng.Bll.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlashEng.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Отримати всіх користувачів
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all users");
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            return Ok(users);
        }

        /// <summary>
        /// Отримати користувача по ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting user with ID: {UserId}", id);
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Отримати користувача по email
        /// </summary>
        [HttpGet("by-email/{email}")]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting user with email: {Email}", email);
            var user = await _userService.GetUserByEmailAsync(email, cancellationToken);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Створити нового користувача
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> CreateUser([FromBody] CreateUserDto createUserDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new user with email: {Email}", createUserDto.Email);
            var userId = await _userService.CreateUserAsync(createUserDto, cancellationToken);

            return CreatedAtAction(nameof(GetUserById), new { id = userId }, userId);
        }

        /// <summary>
        /// Оновити користувача
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            var result = await _userService.UpdateUserAsync(id, updateUserDto, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Видалити користувача
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);
            var result = await _userService.DeleteUserAsync(id, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Отримати налаштування користувача
        /// </summary>
        [HttpGet("{id:int}/settings")]
        public async Task<ActionResult<UserSettingsDto>> GetUserSettings(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting settings for user with ID: {UserId}", id);
            var settings = await _userService.GetUserSettingsAsync(id, cancellationToken);

            if (settings == null)
                return NotFound();

            return Ok(settings);
        }

        /// <summary>
        /// Оновити налаштування користувача
        /// </summary>
        [HttpPut("{id:int}/settings")]
        public async Task<IActionResult> UpdateUserSettings(int id, [FromBody] UserSettingsDto settingsDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating settings for user with ID: {UserId}", id);
            var result = await _userService.UpdateUserSettingsAsync(id, settingsDto, cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
