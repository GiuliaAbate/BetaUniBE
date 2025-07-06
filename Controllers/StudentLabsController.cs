using BetaUni.Models;
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

        
        // PUT: api/StudentLabs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudentLab(int id, StudentLab studentLab)
        {
            if (id != studentLab.Id)
            {
                return BadRequest();
            }

            _context.Entry(studentLab).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentLabExists(id))
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

        // POST: api/StudentLabs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StudentLab>> PostStudentLab(StudentLab studentLab)
        {
            _context.StudentLabs.Add(studentLab);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudentLab", new { id = studentLab.Id }, studentLab);
        }

        // DELETE: api/StudentLabs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentLab(int id)
        {
            var studentLab = await _context.StudentLabs.FindAsync(id);
            if (studentLab == null)
            {
                return NotFound();
            }

            _context.StudentLabs.Remove(studentLab);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentLabExists(int id)
        {
            return _context.StudentLabs.Any(e => e.Id == id);
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

        //Metodo in cui si vanno a prendere tutti i laboratori aggiunti da uno studente
        [HttpGet("LabsByStudent")]
        public async Task<ActionResult<StudentLab>> GetSelectedLabs()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedLabs = await _context.StudentLabs
                .Where(s => s.StudId == studID).ToListAsync();

            if (selectedLabs == null)
            {
                return NotFound("Nessun laboratorio selezionato");
            }

            return Ok(selectedLabs);

        }
        #endregion

        #region POST
        //Metodo per permettere all'utente di iscriversi ad un laboratorio
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
    }
}
