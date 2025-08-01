using BetaUni.Models;
using BetaUni.Other;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

//Controller di base vuoto che avrà metodi di jwt per login con token
namespace BetaUni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private JWTSettings _jwtSettings;

        public LoginController(JWTSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        //Metodo per generare i token quando professore fa login
        private string GenerateProfessorToken(string profID, string email)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey!));
                //Si crea un nuovo oggetto di credenziali di firma, si passa la chiave e si va a cifrare tutto
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                //Si creano i claims specificando il tipo, passando quindi Id, Ruolo ed Email
                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, profID),
                new Claim(ClaimTypes.Role, "Professor"),
                new Claim(ClaimTypes.Email, email)
            };

                //Si crea oggetto token dove si specificano tutte gli attributi importanti
                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.AudienceP,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    signingCredentials: creds
                    );

                var tokenHandler = new JwtSecurityTokenHandler();
                //Il token viene reso una stringa e si scrive il token creato
                string tokenString = tokenHandler.WriteToken(token);

                return tokenString;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Metodo per generare i token quando studente fa login
        private string GenerateStudentToken(string studID, string email)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, studID),
                new Claim(ClaimTypes.Role, "Student"),
                new Claim(ClaimTypes.Email, email)
            };

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.AudienceS,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    signingCredentials: creds
                    );

                var tokenHandler = new JwtSecurityTokenHandler();
                string tokenString = tokenHandler.WriteToken(token);

                return tokenString;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Chiamata per permettere al professore di fare il login
        [HttpPost("ProfLogin")]
        public async Task<IActionResult> ProfessorLogin([FromBody] Login login, [FromServices] IubContext context)
        {
            try
            {
                var prof = await context.Professors
                    .FirstOrDefaultAsync(p => p.Email == login.Email);

                if (prof == null)
                {
                    return Unauthorized(new
                    {
                        field = "Email",
                        message = "Email errata"
                    });
                }

                //Si usa il metodo per verificare la password
                if (!Services.VerifyPassword(login.Password, prof.Password, prof.Salt))
                {
                    return Unauthorized(new
                    {
                        field = "Password",
                        message = "Password errata"
                    });
                }

                var token = GenerateProfessorToken(prof.ProfId, prof.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Chiamata per permettere allo studente  di fare il login
        [HttpPost("StudLogin")]
        public async Task<IActionResult> StudentLogin([FromBody] Login login, [FromServices] IubContext context)
        {
            try
            {
                var stud = await context.Students
                    .FirstOrDefaultAsync(p => p.Email == login.Email);

                if (stud == null)
                {
                    return Unauthorized(new
                    {
                        field = "Email",
                        message = "Email errata"
                    });
                }

                if (!Services.VerifyPassword(login.Password, stud.Password, stud.Salt))
                {
                    return Unauthorized(new
                    {
                        field = "Password",
                        message = "Password errata"
                    });
                }

                var token = GenerateStudentToken(stud.StudId, stud.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
