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
    public class ExamsController : ControllerBase
    {
        private readonly IubContext _context;

        public ExamsController(IubContext context)
        {
            _context = context;
        }

        // GET: api/Exams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exam>>> GetExams()
        {
            return await _context.Exams.ToListAsync();
        }

        // GET: api/Exams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Exam>> GetExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);

            if (exam == null)
            {
                return NotFound();
            }

            return exam;
        }

        // PUT: api/Exams/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExam(int id, Exam exam)
        {
            if (id != exam.ExamId)
            {
                return BadRequest();
            }

            _context.Entry(exam).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(id))
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

        // POST: api/Exams
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Exam>> PostExam(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExam", new { id = exam.ExamId }, exam);
        }

        // DELETE: api/Exams/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExam(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExamExists(int id)
        {
            return _context.Exams.Any(e => e.ExamId == id);
        }

        #region GET

        //Metodo per prendere tutti gli esami prendendo la facoltà
        [HttpGet("GetExams")]
        public async Task<IActionResult> GetExamsFromDepartment()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non trovato");
            }

            var courses = await _context.StudentCourses
              .Where(sc => sc.StudId == studID)
              .Select(sc => sc.CourseId)
              .ToListAsync();

            //var student = await _context.Students
            //    .Include(s => s.Department)
            //    .Where(s => s.StudId.Equals(studID))
            //    .FirstOrDefaultAsync();

            //if (student == null)
            //{
            //    return NotFound("Utente non trovato");
            //}

            var exams = await _context.Exams
                .Include(e => e.Course)
                .Where(e => courses.Contains(e.CourseId))
                .ToListAsync();

            return Ok(exams);
        }

        //Prendere dati esame includendo anche il prof (quindi da tabella ProfCourseExam)
        //v2 di quello sopra
        [Authorize]
        [HttpGet("GetExamsInfo")]
        public async Task<ActionResult<IEnumerable<ExamInfos>>> GetExamsFull()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non trovato");
            }

            var courses = await _context.StudentCourses
            .Where(sc => sc.StudId == studID)
            .Select(sc => sc.CourseId)
            .ToListAsync();

            var exams = await _context.Exams
                .Include(e => e.Course)
                .Include(pc => pc.ProfCourseExams)
                    .ThenInclude(p => p.Prof)
                .Where(e => courses.Contains(e.CourseId))
                .Select(ex => new ExamInfos
                {
                    ExamId = ex.ExamId,
                    Name = ex.Name,
                    Cfu = ex.Cfu,
                    Type = ex.Type,
                    CourseId = ex.CourseId,
                    ProfFullName = ex.ProfCourseExams.FirstOrDefault() != null
                                ? ex.ProfCourseExams.FirstOrDefault()!.Prof.Name 
                                + " " + ex.ProfCourseExams.FirstOrDefault()!.Prof.Surname
                                : null,
                    Date = ex.Date
                }).ToListAsync();

            return Ok(exams);
        }
        #endregion
    }
}

//classe copia con anche il nome del prof e facoltà
public class ExamInfos
{
    public int ExamId { get; set; }
    public string Name { get; set; }
    public int Cfu { get; set; }
    public string Type { get; set; }
    public string CourseId { get; set; }
    public string? ProfFullName { get; set; }
    public DateOnly Date { get; set; }
}