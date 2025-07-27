using BetaUni.Models;
using BetaUni.Other;
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

        // PUT: api/Professors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfessor(string id, Professor professor)
        {
            if (id != professor.ProfId)
            {
                return BadRequest();
            }

            _context.Entry(professor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfessorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Professors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Professor>> PostProfessor(Professor professor)
        {
            _context.Professors.Add(professor);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfessorExists(professor.ProfId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProfessor", new { id = professor.ProfId }, professor);
        }

        // DELETE: api/Professors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessor(string id)
        {
            var professor = await _context.Professors.FindAsync(id);
            if (professor == null)
            {
                return NotFound();
            }

            _context.Professors.Remove(professor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfessorExists(string id)
        {
            return _context.Professors.Any(e => e.ProfId == id);
        }

        //METODI NON DI DEFAULT

        #region GET

        //Metodo con la quale il professore potrà vedere i suoi dati
        [HttpGet("ViewProfessorInfo")]
        public async Task<IActionResult> GetProfessorInfo()
        {
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
    }
}

public class ProfInfos
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
}

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