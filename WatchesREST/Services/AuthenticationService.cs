using WatchLibrary.Models;
using WatchLibrary.Repositories;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Logging;

namespace WatchesREST.Services
{
    public class AuthenticationService
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<AuthenticationService> _logger;
        private const int MaxFailedAttempts = 3;
        private const int LockoutMinutes = 10;

        public AuthenticationService(UserRepository userRepository, ILogger<AuthenticationService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public User? Authenticate(string email, string password, out string message)
        {
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                message = "Invalid email or password.";
                return null;
            }

            //  Tjek om kontoen er låst
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                message = $"Account is locked. Try again at {user.LockoutEnd}";
                return null;
            }

            // Verificer adgangskoden
            if (!Argon2.Verify(user.PasswordHash, password))
            {
                user.FailedAttempts++;
                if (user.FailedAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes * (user.FailedAttempts - MaxFailedAttempts + 1));
                    message = "Account locked due to multiple failed login attempts.";
                }
                else
                {
                    message = "Invalid email or password.";
                }

                _userRepository.Update(user);
                _logger.LogWarning($"Failed login attempt for user {email}. Attempt {user.FailedAttempts}");
                return null;
            }

            //  Nulstil fejl efter succesfuldt login
            if (user.FailedAttempts > 0 || user.LockoutEnd != null)
            {
                user.FailedAttempts = 0;
                user.LockoutEnd = null;
                _userRepository.Update(user);
            }

            _logger.LogInformation($"User {email} logged in successfully.");
            message = "Login successful.";
            return user;
        }
    }
}
