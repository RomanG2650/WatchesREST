using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WatchLibrary.Models;
using WatchLibrary.Repositories;
using WatchesREST.Controllers;
using Microsoft.Extensions.Configuration;

namespace WatchesREST.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        public User GetUserById(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {id} not found.");
            }
            return user;
        }

        public User RegisterUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Password is required");

            if (!PasswordValidator.ValidatePassword(user.Password))
            {
                throw new InvalidOperationException("Password does not meet complexity requirements.");
            }

            // Hash the password before storing it
            user.Password = HashPassword(user.Password);

            // Proceed with user registration
            return _userRepository.Add(user);
        }

        public string Authenticate(string username, string password)
        {
            var user = _userRepository.GetByEmail(username);
            if (user == null || user.Password == null || !VerifyPassword(password, user.Password))
                return string.Empty;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured."));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username ?? throw new InvalidOperationException("Username is null.")) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            // Implement your password hashing logic here
            return password; // Placeholder, replace with actual hashing
        }

        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            // Implement your password verification logic here
            return enteredPassword == storedPasswordHash; // Placeholder, replace with actual verification
        }
    }
}
