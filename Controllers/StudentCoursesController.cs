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
    public class StudentCoursesController : ControllerBase
    {
        private readonly IubContext _context;

        public StudentCoursesController(IubContext context)
        {
            _context = context;
        }

        // PUT: api/StudentCourses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudentCourse(int id, StudentCourse studentCourse)
        {
            if (id != studentCourse.Id)
            {
                return BadRequest();
            }

            _context.Entry(studentCourse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentCourseExists(id))
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

        // POST: api/StudentCourses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StudentCourse>> PostStudentCourse(StudentCourse studentCourse)
        {
            _context.StudentCourses.Add(studentCourse);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudentCourse", new { id = studentCourse.Id }, studentCourse);
        }

        // DELETE: api/StudentCourses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudentCourse(int id)
        {
            var studentCourse = await _context.StudentCourses.FindAsync(id);
            if (studentCourse == null)
            {
                return NotFound();
            }

            _context.StudentCourses.Remove(studentCourse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentCourseExists(int id)
        {
            return _context.StudentCourses.Any(e => e.Id == id);
        }

        #region GET

        // GET: api/StudentCourses --> si vedono tutte le iscrizioni ai corsi, di tutte le facoltà e studenti
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentCourse>>> GetStudentCourses()
        {
            return await _context.StudentCourses.ToListAsync();
        }

        // GET: api/StudentCourses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudentCourse>> GetStudentCourse(int id)
        {
            var studentCourse = await _context.StudentCourses.FindAsync(id);

            if (studentCourse == null)
            {
                return NotFound();
            }

            return studentCourse;
        }

        //Metodo in cui si vanno a prendere tutti i corsi aggiunti da uno studente
        [HttpGet("CoursesByStudent")]
        public async Task<ActionResult<StudentCourse>> GetSelectedCourses()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedCourses = await _context.StudentCourses
                .Where(s => s.StudId == studID).ToListAsync();

            if(selectedCourses == null)
            {
                return NotFound("Nessun corso selezionato");
            }

            return Ok(selectedCourses);

        }

        #endregion 

        #region POST
        //Metodo per permettere allo studente di iscriversi e partecipare ad un corso
        //Quindi l'utente si fa un piano di studi, in cui per prima cosa sceglie i corsi della propria facoltà
        //Si deve prendere automaticamente id utente, id corso e metterli nella tabella
        [HttpPost("CourseRegistration/{courseId}")]
        public async Task<IActionResult> AddToStudyPlan(string courseId)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if(course == null)
            {
                return NotFound("Corso non trovato");
            }

            bool alreadyRegistered = await _context.StudentCourses
                .AnyAsync(s => s.StudId.Equals(studID) && s.CourseId.Equals(courseId));
            if (alreadyRegistered)
            {
                return BadRequest("Corso già scelto");
            }

            var chosenCourse = new StudentCourse
            {
                StudId = studID,
                CourseId = courseId,
                DepartmentId = course.DepartmentId
            };

            _context.StudentCourses.Add(chosenCourse);
            await _context.SaveChangesAsync();

            return Ok(chosenCourse);

        }
        #endregion

        #region DELETE
        //Disiscriversi da un corso
        [HttpDelete("CourseUnsubscribe/{regId}")]
        public async Task<IActionResult> DeleteCourseRegistration(int regId)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var courseRegistration = await _context.StudentCourses.FindAsync(regId);
            if (courseRegistration == null)
            {
                return NotFound();
            }

            _context.StudentCourses.Remove(courseRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
