using Microsoft.AspNetCore.Mvc;
using WatchLibrary.Models;
using WatchLibrary.Repositories;
using WatchLibrary.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WatchesREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly UserRepository _users;
        private readonly IConfiguration _config;

        public LoginController(AuthenticationService authService, UserRepository users, IConfiguration config)
        {
            _authService = authService;
            _users = users;
            _config = config;
        }

        [HttpPost]
        public ActionResult<object> Login([FromBody] LoginRequest request)
        {
            var user = _authService.Authenticate(request.Email, request.Password, out string message);

            if (user == null)
            {
                return BadRequest(message); // 🚨 Forhindrer null-reference fejl
            }

            var token = GenerateJwtToken(user); // 🛠️ Nu kaldes dette KUN hvis user != null
            return Ok(new { token });
        }


        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"]);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
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
    }
}
