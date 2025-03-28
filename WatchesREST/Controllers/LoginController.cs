using Microsoft.AspNetCore.Mvc;
using WatchLibrary.Models;
using WatchLibrary.Repositories;
using WatchesREST.Services;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WatchesREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly UserRepository _users;
        private readonly IConfiguration _config;
        private readonly ILogger<LoginController> _logger; // For logning

        public LoginController(AuthenticationService authService, UserRepository users, IConfiguration config, ILogger<LoginController> logger)
        {
            _authService = authService;
            _users = users;
            _config = config;
            _logger = logger;
        }

        // eksisterende login-metode
        [HttpPost]
        public ActionResult<object> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            try
            {
                var user = _authService.Authenticate(request.Email, request.Password, out string message);

                if (user == null)
                {
                    return BadRequest(message);
                }

                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Password validation failed: {ex.Message}");
            }
        }

        // Ny metode til ændring af adgangskode
        [HttpPut("change-password")]
        public ActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Email, old password, and new password are required.");
            }

            var user = _users.GetByEmail(request.Email); // Hent brugeren fra databasen via repository
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Verificer den gamle adgangskode
            string message;
            var authenticatedUser = _authService.Authenticate(user.Email ?? string.Empty, request.OldPassword, out message);

            if (authenticatedUser == null)
            {
                return BadRequest("Old password is incorrect.");
            }

            // Valider den nye adgangskode
            try
            {
                _authService.ValidatePasswordComplexity(request.NewPassword); // Tjek om den nye adgangskode opfylder kravene
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Password validation failed: {ex.Message}");
            }

            // Opdater adgangskoden i databasen
            user.PasswordHash = Argon2.Hash(request.NewPassword); // Hash den nye adgangskode
            _users.Update(user); // Opdater brugeren i databasen

            // Log hændelsen
            _logger.LogInformation($"User with email {user.Email} changed their password.");

            return Ok("Password changed successfully.");
        }

        private string GenerateJwtToken(User user)
        {
            var secret = _config["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("JWT Secret is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(secret);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddHours(1),
                claims: claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class LoginRequest
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
        }

        // Model for ændring af adgangskode
        public class ChangePasswordRequest
        {
            public required string Email { get; set; }
            public required string OldPassword { get; set; }
            public required string NewPassword { get; set; }
        }
    }
}

