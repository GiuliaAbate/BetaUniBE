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

        private bool ProfCourseExamExists(int id)
        {
            return _context.ProfCourseExams.Any(e => e.Id == id);
        }

        #region GET
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

        //Metodo per far sì che il professore possa vedere gli studenti iscritti al suo corso
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
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
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
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
                .Select(sc => new
                {
                    sc.Stud.StudId,
                    sc.Stud.Name,
                    sc.Stud.Surname,
                    sc.Stud.Email,
                    sc.RegistrationDate
                })
                .ToListAsync();

            return Ok(students);
        }

        //Metodo per far sì che il professore possa vedere gli studenti iscritti al suo laboratorio
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
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

        //Metodo per far sì che il professore possa vedere i suoi esami futuri di cui è professore
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
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

        //Metodo per prendere i corsi che il professore ha scelto
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpGet("ProfSelectedCourses")]
        public async Task<IActionResult> GetProfCourses()
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedCourses = await _context.ProfCourseExams
                .Where(p => p.ProfId == profID)
                .Include(c => c.Course)
                    .ThenInclude(cl => cl.Classrooms)
                .Select(c => new
                {
                    c.Id,
                    c.CourseId,
                    c.ExamId,
                    c.Course.Name,
                    c.Course.StartDate,
                    c.Course.EndDate,
                    Classrooms = c.Course.Classrooms.FirstOrDefault() != null
                        ? c.Course.Classrooms.FirstOrDefault()!.Name
                        + " " + c.Course.Classrooms.FirstOrDefault()!.Number
                        : null
                }).ToListAsync();

            if (selectedCourses == null)
            {
                return NotFound("Nessun corso selezionato");
            }

            return Ok(selectedCourses);
        }


        //Metodo per prendere gli esami a cui il prof è iscritto
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpGet("ProfSelectedExams")]
        public async Task<IActionResult> GetProfExams()
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedCourses = await _context.ProfCourseExams
                .Where(p => p.ProfId == profID)
                .Include(c => c.Exam)
                .Select(c => new
                {
                    c.Id,
                    c.CourseId,
                    c.ExamId,
                    c.Exam.Name,
                    c.Exam.Cfu,
                    c.Exam.Type,
                    c.Exam.Date
                }).ToListAsync();

            if (selectedCourses == null)
            {
                return NotFound("Nessun corso selezionato");
            }

            return Ok(selectedCourses);
        }

        //Metodo per prendere le registrazioni del professore al corso/esame
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpGet("ProfessorCourseExams")]
        public async Task<IActionResult> GetProfessorCourseExams()
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var courses = await _context.ProfCourseExams
                .Where(e => e.ProfId == profID)
                .ToListAsync();

            return Ok(courses);
        }
        #endregion

        #region POST
        //Metodo per permettere al professore di aggiungere al suo piano didattico il corso (ed esame insieme)
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
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
        //Disiscrizione dal corso ed esame scelti
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
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
