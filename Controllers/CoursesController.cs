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
    public class CoursesController : ControllerBase
    {
        private readonly IubContext _context;

        public CoursesController(IubContext context)
        {
            _context = context;
        }

        #region GET
        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            return await _context.Courses.ToListAsync();
        }

        // GET: api/Courses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(string id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        //Metodo per prendere tutti i corsi data una facoltà (senza utente loggato)
        [HttpGet("GetCoursesByDep/{depID}")]
        public async Task<IActionResult> GetCoursesByDep(string depID)
        {
            var course = await _context.Courses
                .Where(c => c.DepartmentId == depID)
                .Select(c => new
                {
                    c.Name,
                    c.StartDate,
                    c.EndDate,
                    Classrooms = c.Classrooms.Select(cl => new
                    {
                        cl.Name,
                        cl.Number,
                        cl.MaxCapacity,
                        cl.CourseId
                    }).ToList()
                })
                .ToListAsync();

            if (course == null)
            {
                return NotFound("Nessun corso trovato");
            }

            return Ok(course);
        }

        //Metodo per prendere tutti i corsi guardando la facoltà dello studente
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpGet("DepCourses")]
        public async Task<IActionResult> GetCoursesFromDepartment()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var student = await _context.Students
                .Include(s => s.Department)
                .Where(s => s.StudId.Equals(studID))
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound("Studente non trovato");
            }

            var course = await _context.Courses
                .Where(c => c.DepartmentId.Equals(student.DepartmentId))
                .ToListAsync();

            return Ok(course);
        }

        //Metodo per prendere tutti i corsi ed esami collegati guardando la facoltà del professore
        [Authorize(AuthenticationSchemes = "ProfessorScheme")]
        [HttpGet("ProfDepCoursesExams")]
        public async Task<IActionResult> GetCoursesAndExams()
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var professor = await _context.Professors
                .Include(p => p.Department)
                .Where(p => p.ProfId.Equals(profID))
                .FirstOrDefaultAsync();

            if (professor == null)
            {
                return NotFound("Professore non trovato");
            }

            //Si prendono i corsi controllando id della facoltà 
            var result = await _context.Courses
                .Where(c => c.DepartmentId.Equals(professor.DepartmentId))
                //Poi si vanno a prendere anche gli esami controllando che id del corso sia lo stesso
                .SelectMany(c => _context.Exams
                    .Where(e => e.CourseId == c.CourseId)
                    //Poi si selezionano le informazioni necessarie di entrambi
                    .Select(e => new
                    {
                        c.CourseId,
                        c.Name,
                        c.StartDate,
                        c.EndDate,
                        ExamName = e.Name,
                        e.Cfu,
                        e.Type,
                        e.ExamId
                    }))
                .ToListAsync();

            return Ok(result);
        }
        #endregion

        #region POST
        // POST: api/Courses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(Course course)
        {
            _context.Courses.Add(course);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CourseExists(course.CourseId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCourse", new { id = course.CourseId }, course);
        }
        #endregion

        #region PUT
        // PUT: api/Courses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(string id, Course course)
        {
            if (id != course.CourseId)
            {
                return BadRequest();
            }

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
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
        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(string id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        private bool CourseExists(string id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }

    }
}
