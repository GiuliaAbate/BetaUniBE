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
    public class StudentLabsController : ControllerBase
    {
        private readonly IubContext _context;

        public StudentLabsController(IubContext context)
        {
            _context = context;
        }

        #region GET
        // GET: api/StudentLabs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentLab>>> GetStudentLabs()
        {
            return await _context.StudentLabs.ToListAsync();
        }

        // GET: api/StudentLabs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentLab>> GetStudentLab(int id)
        {
            var studentLab = await _context.StudentLabs.FindAsync(id);

            if (studentLab == null)
            {
                return NotFound();
            }

            return studentLab;
        }

        //Chiamata in cui si vanno a prendere tutti i laboratori aggiunti da uno studente
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpGet("LabsByStudent")]
        public async Task<ActionResult<IEnumerable<LabInfos>>> GetSelectedLabs()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            //Si prendono le registrazioni degli studenti ai laboratori 
            var selectedLabs = await _context.StudentLabs
                //Si controlla id studente
                .Where(s => s.StudId == studID)
                //Si includono informazioni del laboratorio e la classe
                .Include(s => s.Lab)
                    .ThenInclude(l => l.Classrooms)
                .Include(s => s.Lab)
                //Poi si includono le registrazioni dei professori ai laboratori e quindi informazioni del prof
                    .ThenInclude(l => l.ProfessorLabs)
                        .ThenInclude(pl => pl.Prof)
                .Select(s => new LabInfos
                {
                    Id = s.Id,
                    LabId = s.Lab.LabId,
                    Name = s.Lab.Name,
                    Attendance = s.Lab.Attendance,
                    StartDate = s.Lab.StartDate,
                    EndDate = s.Lab.EndDate,
                    //Si prende nome completo del prof
                    ProfFullName = s.Lab.ProfessorLabs.FirstOrDefault() != null
                        ? s.Lab.ProfessorLabs.FirstOrDefault()!.Prof.Name
                        + " " + s.Lab.ProfessorLabs.FirstOrDefault()!.Prof.Surname
                        : null,
                    //Si prende nome e numero dell'aula
                    Classrooms = s.Lab.Classrooms.FirstOrDefault() != null
                        ? s.Lab.Classrooms.FirstOrDefault()!.Name
                        + " " + s.Lab.Classrooms.FirstOrDefault()!.Number
                        : null
                })
                .ToListAsync();

            if (selectedLabs == null)
            {
                return NotFound("Nessun laboratorio selezionato");
            }

            return Ok(selectedLabs);
        }


        #endregion

        #region POST
        //Metodo per permettere all'utente di iscriversi ad un laboratorio
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpPost("LabRegistration/{labId}")]
        public async Task<IActionResult> AddLabToStudyPlan(int labId)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var lab = await _context.Laboratories.FindAsync(labId);
            if (lab == null)
            {
                return NotFound("Laboratorio non trovato");
            }

            bool alreadyRegistered = await _context.StudentLabs
                .AnyAsync(s => s.StudId.Equals(studID) && s.LabId == labId);
            if (alreadyRegistered)
            {
                return BadRequest("Laboratorio già scelto");
            }

            var chosenLab = new StudentLab
            {
                StudId = studID,
                LabId = labId,
                DepartmentId = lab.DepartmentId
            };

            _context.StudentLabs.Add(chosenLab);
            await _context.SaveChangesAsync();

            return Ok(chosenLab);

        }
        #endregion

        #region DELETE
        //Disiscriversi da un laboratorio
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpDelete("LabUnsubscribe/{regId}")]
        public async Task<IActionResult> DeleteLabRegistration(int regId)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var labRegistration = await _context.StudentLabs.FindAsync(regId);
            if (labRegistration == null)
            {
                return NotFound();
            }

            _context.StudentLabs.Remove(labRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool StudentLabExists(int id)
        {
            return _context.StudentLabs.Any(e => e.Id == id);
        }
    }
}

public class LabInfos
{
    public int Id { get; set; }
    public int LabId { get; set; }
    public string Name { get; set; }
    public string Attendance { get; set; }
    public string? ProfFullName { get; set; }
    public string? Classrooms { get; set; }
    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

}
