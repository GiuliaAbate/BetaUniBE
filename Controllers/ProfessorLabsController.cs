using BetaUni.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BetaUni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorLabsController : ControllerBase
    {
        private readonly IubContext _context;

        public ProfessorLabsController(IubContext context)
        {
            _context = context;
        }

        #region GET 
        // GET: api/ProfessorLabs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfessorLab>>> GetProfessorLabs()
        {
            return await _context.ProfessorLabs.ToListAsync();
        }

        // GET: api/ProfessorLabs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfessorLab>> GetProfessorLab(int id)
        {
            var professorLab = await _context.ProfessorLabs.FindAsync(id);

            if (professorLab == null)
            {
                return NotFound();
            }

            return professorLab;
        }

        //Metodo per prendere i laboratori a cui il prof è iscritto
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpGet("ProfSelectedLabs")]
        public async Task<IActionResult> GetProfLabs()
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedLabs = await _context.ProfessorLabs
                .Where(p => p.ProfId == profID)
                .Include(l => l.Lab)
                    .ThenInclude(cl => cl.Classrooms)
                .Select(l => new
                {
                    l.Id,
                    l.LabId,
                    l.Lab.Name,
                    l.Lab.Attendance,
                    l.Lab.StartDate,
                    l.Lab.EndDate,
                    Classrooms = l.Lab.Classrooms.FirstOrDefault() != null
                        ? l.Lab.Classrooms.FirstOrDefault()!.Name
                        + " " + l.Lab.Classrooms.FirstOrDefault()!.Number
                        : null
                }).ToListAsync();

            if (selectedLabs == null)
            {
                return NotFound("Nessun corso selezionato");
            }

            return Ok(selectedLabs);
        }
        #endregion

        #region POST
        //Metodo per permettere al prof di scegliere un laboratorio
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpPost("ProfAddLab/{labId}")]
        public async Task<IActionResult> AddLab(int labId)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var lab = await _context.Laboratories.FindAsync(labId);
            if (lab == null)
            {
                return NotFound("Laboratorio non trovato");
            }

            bool alreadyRegistered = await _context.ProfessorLabs
                .AnyAsync(p => p.ProfId.Equals(profID) && p.LabId == labId);

            if (alreadyRegistered)
            {
                return BadRequest("Laboratorio già scelto");
            }

            var chosenLab = new ProfessorLab
            {
                ProfId = profID,
                LabId = labId,
                DepartmentId = lab.DepartmentId
            };

            _context.ProfessorLabs.Add(chosenLab);
            await _context.SaveChangesAsync();

            return Ok(chosenLab);
        }
        #endregion

        #region DELETE
        //Chiamata per permettere al professore di disiscriversi da un laboratorio
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpDelete("DeleteLab/{id}")]
        public async Task<IActionResult> DeleteLabRegistration(int id)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var labRegistration = await _context.ProfessorLabs.FindAsync(id);
            if (labRegistration == null)
            {
                return NotFound();
            }

            _context.ProfessorLabs.Remove(labRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool ProfessorLabExists(int id)
        {
            return _context.ProfessorLabs.Any(e => e.Id == id);
        }
    }
}
