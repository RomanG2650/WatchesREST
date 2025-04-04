using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WatchLibrary.Models;

namespace WatchesREST.Services
{
    public class JwtService
    {
        private readonly string _key;

        public JwtService(IConfiguration configuration)
        {
            _key = configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(_key))
            {
                // Stop programmet hvis JWT-nøglen mangler – vigtigt for sikkerhed
                throw new InvalidOperationException("JWT key er ikke sat korrekt i appsettings.json (Jwt:Key mangler eller er tom)");
            }
        }

        public string GenerateToken(User user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var keyBytes = Encoding.ASCII.GetBytes(_key);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Email ?? "ukendt"),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
						new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // brugerens ID
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(keyBytes),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                // Giv detaljeret fejlbesked 
                throw new Exception("Fejl under generering af JWT-token: " + ex.Message, ex);
            }
        }

        // Brug til at teste om JWT-nøglen læses korrekt 
        public string DebugKey() => _key;
    }
}

