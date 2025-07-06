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
    public class ExamRegistrationsController : ControllerBase
    {
        private readonly IubContext _context;

        public ExamRegistrationsController(IubContext context)
        {
            _context = context;
        }

        #region GET
        // GET: api/ExamRegistrations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamRegistration>>> GetExam()
        {
            return await _context.ExamRegistrations.ToListAsync();
        }

        // GET: api/ExamRegistrations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamRegistration>> GetExamRegistration(int id)
        {
            var examRegistration = await _context.ExamRegistrations.FindAsync(id);

            if (examRegistration == null)
            {
                return NotFound();
            }

            return examRegistration;
        }

        //Metodo in cui si vanno a prendere tutti gli esami a cui lo studente è iscritto
        [HttpGet("ExamsByStudent")]
        public async Task<ActionResult<ExamRegistration>> GetSelectedExams()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedExams = await _context.ExamRegistrations
                .Where(s => s.StudId == studID).ToListAsync();

            if (selectedExams == null)
            {
                return NotFound("Nessun esame selezionato");
            }

            return Ok(selectedExams);

        }
        #endregion

        #region POST
        //Metodo per permettere allo studente di iscriversi ad un esame
        [HttpPost("Registration/{examId}")]
        public async Task<IActionResult> RegisterToExam(int examId)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var exam = await _context.Exams.FindAsync(examId);
            if (exam == null)
            {
                return NotFound("Esame non trovato");
            }

            bool alreadyRegistered = await _context.ExamRegistrations
                .AnyAsync(s => s.StudId.Equals(studID) && s.CourseId.Equals(examId));
            if (alreadyRegistered)
            {
                return BadRequest("Iscrizione all'esame già avvenuta");
            }

            var chosenExam = new ExamRegistration
            {
                StudId = studID,
                ExamId = examId,
                CourseId = exam.CourseId,
                DepartmentId = exam.DepartmentId,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.ExamRegistrations.Add(chosenExam);
            await _context.SaveChangesAsync();

            return Ok(chosenExam);

        }

        // POST: api/ExamRegistrations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ExamRegistration>> PostExamRegistration(ExamRegistration examRegistration)
        {
            _context.ExamRegistrations.Add(examRegistration);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExamRegistration", new { id = examRegistration.Id }, examRegistration);
        }

        #endregion

        #region PUT
        // PUT: api/ExamRegistrations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExamRegistration(int id, ExamRegistration examRegistration)
        {
            if (id != examRegistration.Id)
            {
                return BadRequest();
            }

            _context.Entry(examRegistration).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamRegistrationExists(id))
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
        #endregion

        #region DELETE
        //Disiscriversi da un esame
        [HttpDelete("ExamUnsubscribe/{regId}")]
        public async Task<IActionResult> DeleteExamRegistration(int regId)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var examRegistration = await _context.ExamRegistrations.FindAsync(regId);
            if (examRegistration == null)
            {
                return NotFound();
            }

            _context.ExamRegistrations.Remove(examRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ExamRegistrations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            var examRegistration = await _context.ExamRegistrations.FindAsync(id);
            if (examRegistration == null)
            {
                return NotFound();
            }

            _context.ExamRegistrations.Remove(examRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool ExamRegistrationExists(int id)
        {
            return _context.ExamRegistrations.Any(e => e.Id == id);
        }
    }
}