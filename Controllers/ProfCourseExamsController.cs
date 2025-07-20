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
    public class ProfCourseExamsController : ControllerBase
    {
        private readonly IubContext _context;

        public ProfCourseExamsController(IubContext context)
        {
            _context = context;
        }

        // GET: api/ProfCourseExams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfCourseExam>>> GetProfCourseExams()
        {
            return await _context.ProfCourseExams.ToListAsync();
        }

        // GET: api/ProfCourseExams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfCourseExam>> GetProfCourseExam(int id)
        {
            var profCourseExam = await _context.ProfCourseExams.FindAsync(id);

            if (profCourseExam == null)
            {
                return NotFound();
            }

            return profCourseExam;
        }

        // PUT: api/ProfCourseExams/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfCourseExam(int id, ProfCourseExam profCourseExam)
        {
            if (id != profCourseExam.Id)
            {
                return BadRequest();
            }

            _context.Entry(profCourseExam).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfCourseExamExists(id))
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

        // POST: api/ProfCourseExams
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProfCourseExam>> PostProfCourseExam(ProfCourseExam profCourseExam)
        {
            _context.ProfCourseExams.Add(profCourseExam);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProfCourseExam", new { id = profCourseExam.Id }, profCourseExam);
        }

        // DELETE: api/ProfCourseExams/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfCourseExam(int id)
        {
            var profCourseExam = await _context.ProfCourseExams.FindAsync(id);
            if (profCourseExam == null)
            {
                return NotFound();
            }

            _context.ProfCourseExams.Remove(profCourseExam);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfCourseExamExists(int id)
        {
            return _context.ProfCourseExams.Any(e => e.Id == id);
        }

        #region GET
        //Metodo per far sì che il professore possa vedere gli studenti iscritti al suo corso/esame
        [HttpGet("StudentByCourse/{id}")]
        public async Task<IActionResult> GetStudentsByCourse(int id)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
                return Unauthorized("Utente non autenticato");

            var profCourseExam = await _context.ProfCourseExams
                .FirstOrDefaultAsync(p => p.Id == id && p.ProfId == profID);

            if (profCourseExam == null)
                return NotFound("Associazione corso-esame non trovata");

            var students = await _context.StudentCourses
                .Where(sc => sc.CourseId == profCourseExam.CourseId)
                .Include(sc => sc.Stud)
                .Select(sc => sc.Stud)
                .ToListAsync();

            return Ok(students);
        }

        //Metodo per far sì che il professore possa vedere gli studenti iscritti al suo esame
        [HttpGet("StudentsByExam/{id}")]
        public async Task<IActionResult> GetStudentsByExam(int id)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
                return Unauthorized("Utente non autenticato");

            var profCourseExam = await _context.ProfCourseExams
                .FirstOrDefaultAsync(p => p.Id == id && p.ProfId == profID);

            if (profCourseExam == null)
                return NotFound("Associazione corso-esame non trovata");

            var students = await _context.ExamRegistrations
                .Where(sc => sc.ExamId == profCourseExam.ExamId)
                .Include(sc => sc.Stud)
                .Select(sc => sc.Stud)
                .ToListAsync();

            return Ok(students);
        }

        //Metodo per far sì che il professore possa vedere gli studenti iscritti al suo laboratorio
        [HttpGet("StudentsByLab/{id}")]
        public async Task<IActionResult> GetStudentsByLab(int id)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
                return Unauthorized("Utente non autenticato");

            var profLab = await _context.ProfessorLabs
                .FirstOrDefaultAsync(p => p.Id == id && p.ProfId == profID);

            if (profLab == null)
                return NotFound();

            var students = await _context.StudentLabs
                .Where(sc => sc.LabId == profLab.LabId)
                .Include(sc => sc.Stud)
                .Select(sc => sc.Stud)
                .ToListAsync();

            return Ok(students);
        }

        [Authorize]
        [HttpGet("ProfFutureExams")]
        public async Task<IActionResult> GetProfFutureExams()
        {
            var profId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profId))
            {
                return Unauthorized("Utente non autenticato");
            }

            var exams = await _context.ProfCourseExams
                .Where(p => p.ProfId == profId)
                .Include(e => e.Exam)
                .Select(e => new
                {
                    e.Exam.ExamId,
                    e.Exam.Name,
                    e.Exam.Cfu,
                    e.Exam.Type,
                    e.Exam.Date
                }).ToListAsync();

            if (exams == null)
            {
                return NotFound("Nessun esame selezionato");
            }

            return Ok(exams);
        }
        #endregion

        #region POST
        [Authorize]
        [HttpPost("AddCourseExamProf/{courseId}/{examId}")]
        public async Task<IActionResult> AddExamCourse(string courseId, int examId)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var exam = await _context.Exams
                .Where(e=>e.ExamId == examId && e.CourseId == courseId)
                .FirstOrDefaultAsync();

            if (exam == null)
            {
                return NotFound("Esame non trovato");
            }

            bool alreadyExists = await _context.ProfCourseExams
                .AnyAsync(p => p.ExamId == exam.ExamId
                && p.CourseId == exam.CourseId
                && p.ProfId == profID);

            if (alreadyExists)
            {
                return BadRequest("Corso ed esame già aggiunto");
            }

            var profCourseExam = new ProfCourseExam
            {
                ExamId = exam.ExamId,
                CourseId = exam.CourseId,
                ProfId = profID,
                DepartmentId = exam.DepartmentId
            };

            _context.ProfCourseExams.Add(profCourseExam);
            await _context.SaveChangesAsync();

            return Ok(profCourseExam);

        }
        #endregion

        #region DELETE
        //Togliere esame e corso scelti
        [Authorize]
        [HttpDelete("DeleteCourseExam/{id}")]
        public async Task<IActionResult> DeleteCourseRegistration(int id)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var courseExamRegistration = await _context.ProfCourseExams.FindAsync(id);
            if (courseExamRegistration == null)
            {
                return NotFound();
            }

            _context.ProfCourseExams.Remove(courseExamRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
