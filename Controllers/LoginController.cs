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

        [HttpPost("ProfLogin")]
        public async Task<IActionResult> ProfessorLogin([FromBody] Login login, [FromServices] IubContext context)
        {
            try
            {
                var prof = await context.Professors
                    .FirstOrDefaultAsync(p => p.Email == login.Email);

                if (prof == null)
                {
                    return Unauthorized("Email o password non validi");
                }

                //Si usa il metodo per verificare la password
                if (!Services.VerifyPassword(login.Password, prof.Password, prof.Salt))
                {
                    return Unauthorized("Email o password non validi");
                }

                var token = GenerateProfessorToken(prof.ProfId, prof.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("StudLogin")]
        public async Task<IActionResult> StudentLogin([FromBody] Login login, [FromServices] IubContext context)
        {
            try
            {
                var stud = await context.Students
                    .FirstOrDefaultAsync(p => p.Email == login.Email);

                if (stud == null)
                {
                    return Unauthorized("Email o password non validi");
                }

                if (!Services.VerifyPassword(login.Password, stud.Password, stud.Salt))
                {
                    return Unauthorized("Email o password non validi");
                }

                var token = GenerateStudentToken(stud.StudId, stud.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Metodo per capire se l'utente è autenticato o meno
        [HttpGet("IsUserLogged")]
        public IActionResult IsLogged() //non si mette async perchè non si fa chiamata a DB
        {
            //Si verifica che l'utente sia già autenticato
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                //Si prendono id e ruolo dai claim types
                var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                //Si indica che utente è autenticato
                var authenticated = new
                {
                    authenticated = true,
                    userID,
                    role
                };

                return Ok(authenticated);
            } else
            {
                return Unauthorized();
            }
        } 


        //Metodo per prendere id da token
        public static string? GetIDFromToken(string token)
        {
            try
            {
                //Si crea nuovo handler
                var handler = new JwtSecurityTokenHandler();

                //Si legge il token passato
                var jwt = handler.ReadJwtToken(token);
                //Si cerca subject tra i claims di jwt e si va a prendere il valore
                var userID = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub)?.Value;
                return userID;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
