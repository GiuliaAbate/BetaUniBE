using BetaUni.Models;
using BetaUni.Other;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BetaUni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorsController : ControllerBase
    {
        private readonly IubContext _context;
        private readonly Services _services;

        public ProfessorsController(IubContext context, Services services)
        {
            _context = context;
            _services = services;
        }

        #region GET

        // GET: api/Professors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Professor>>> GetProfessors()
        {
            return await _context.Professors.ToListAsync();
        }

        // GET: api/Professors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Professor>> GetProfessor(string id)
        {
            var professor = await _context.Professors.FindAsync(id);

            if (professor == null)
            {
                return NotFound();
            }

            return professor;
        }

        //Metodo con la quale il professore potrà vedere i suoi dati
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpGet("ViewProfessorInfo")]
        public async Task<IActionResult> GetProfessorInfo()
        {
            //Come per lo studente si va a cercare che il professore sia connesso
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var professor = await _context.Professors
                .Include(pd => pd.Department)
                .Where(p => p.ProfId == profID)
                .Select(p => new
                {
                    p.ProfId,
                    p.Name,
                    p.Surname,
                    p.BirthDate,
                    p.Email,
                    p.PhoneNumber,
                    p.DepartmentId,
                    p.EnrollmentDate,
                    DepartmentName = p.Department.Name
                }).FirstOrDefaultAsync();

            return Ok(professor);
        }
        #endregion

        #region POST

        //Registrazione del professore
        [HttpPost("ProfRegistration")]
        public async Task<ActionResult<Professor>> SignProfessor(ProfessorRegistration professor)
        {
            try
            {
                var existingProf = await _context.Professors
                    .Where(p => p.Email == professor.Email)
                    .FirstOrDefaultAsync();

                if (existingProf != null)
                {
                    return BadRequest("Email già registrata.");
                }
                else
                {
                    var encrypted = Services.SaltEncryption(professor.Password);
                    Professor prof = new Professor
                    {
                        ProfId = _services.GenerateProfId(),
                        Name = professor.Name,
                        Surname = professor.Surname,
                        BirthDate = professor.BirthDate,
                        Email = professor.Email,
                        Password = encrypted.Key,
                        Salt = encrypted.Value,
                        PhoneNumber = professor.PhoneNumber,
                        DepartmentId = professor.DepartmentId,
                        EnrollmentDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _context.Professors.Add(prof);
                    await _context.SaveChangesAsync();

                    return Ok(prof);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #endregion

        #region PUT
        // Chiamata che permette al professore di aggiornare numero di telefono e password
        [HttpPut("UpdateProfessor")]
        public async Task<IActionResult> UpdateProfInfos(ProfInfos profInfos)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var existingProfessor = await _context.Professors.FindAsync(profID);
            if (existingProfessor == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(profInfos.PhoneNumber) &&
                profInfos.PhoneNumber != existingProfessor.PhoneNumber)
            {
                existingProfessor.PhoneNumber = profInfos.PhoneNumber;
            }

            // Si cripta di nuovo la password quando si va a cambiare
            if (!string.IsNullOrEmpty(profInfos.Password))
            {
                var encrypted = Services.SaltEncryption(profInfos.Password);
                existingProfessor.Password = encrypted.Key;
                existingProfessor.Salt = encrypted.Value;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        private bool ProfessorExists(string id)
        {
            return _context.Professors.Any(e => e.ProfId == id);
        }

    }
}

//Classe che contiene sono i dati che si possono cambiare
public class ProfInfos
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
}

//Classe che contiene i dati richiesti per la registrazione
public class ProfessorRegistration
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Surname { get; set; }

    [Required]
    public DateOnly BirthDate { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string DepartmentId { get; set; }
}