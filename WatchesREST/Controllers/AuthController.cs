using Microsoft.AspNetCore.Mvc;
using WatchLibrary.Models;
using WatchLibrary.Repositories;
using WatchesREST.Services;
using Microsoft.AspNetCore.Http;

namespace WatchesREST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly JwtService _jwtService;

        public AuthController(UserRepository userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }


        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]            // Ved succesfuldt login
        [ProducesResponseType(StatusCodes.Status400BadRequest)]    // Ved dårligt input (valideringsfejl)
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]  // Ved forkert login eller konto låst
        [ProducesResponseType(StatusCodes.Status403Forbidden)]     // Hvis adgang nægtes af anden grund
        public IActionResult Login([FromBody] LoginRequest model)
        {
            // 1. Valider input
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Email og password skal udfyldes.");

            // 2. Find bruger baseret på email
            var user = _userRepository.GetByEmail(model.Email);
            if (user == null)
                return Unauthorized("Brugeren findes ikke.");

            // 3. Tjek om kontoen er midlertidigt låst
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                return Unauthorized($"Kontoen er midlertidigt låst til {user.LockoutEnd.Value}.");

            try
            {
                // 4. Prøv at autentificere bruger (kaster exception hvis forkert kode)
                var authenticatedUser = _userRepository.AuthenticateUser(model.Email, model.Password);

                // 5. Login lykkedes → nulstil loginforsøg
                _userRepository.ResetLoginAttempts(user);

                // 6. DEBUG: Udskriv JWT nøgle 
                Console.WriteLine("JWT Secret (debug): " + _jwtService.DebugKey());

                // 7. Generér token
                var token = _jwtService.GenerateToken(authenticatedUser);

                // 8. Returnér token til klienten
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                // 9. Login fejlede, registrér mislykket forsøg
                _userRepository.RegisterFailedLogin(user);

                // 10. Returnér fejlen til klienten
                return Unauthorized(ex.Message); // Fx: "Forkert adgangskode."
            }
        }

    }
}

