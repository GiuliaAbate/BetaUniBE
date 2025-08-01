﻿using BetaUni.Models;
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
    public class StudentCoursesController : ControllerBase
    {
        private readonly IubContext _context;

        public StudentCoursesController(IubContext context)
        {
            _context = context;
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
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpGet("CoursesByStudent")]
        public async Task<ActionResult<IEnumerable<CourseInfos>>> GetSelectedCourses()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedCourses = await _context.StudentCourses
                .Where(s => s.StudId == studID)
                .Include(c => c.Course)
                    .ThenInclude(cl => cl.Classrooms)
                 .Include(c => c.Course)
                    .ThenInclude(pc => pc.ProfCourseExams)
                    .ThenInclude(p => p.Prof)
                .Select(c => new CourseInfos
                {
                    Id = c.Id,
                    CourseId = c.Course.CourseId,
                    Name = c.Course.Name,
                    StartDate = c.Course.StartDate,
                    EndDate = c.Course.EndDate,
                    ProfFullName = c.Course.ProfCourseExams.FirstOrDefault() != null
                        ? c.Course.ProfCourseExams.FirstOrDefault()!.Prof.Name 
                        + " " + c.Course.ProfCourseExams.FirstOrDefault()!.Prof.Surname
                        : null,
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

        #endregion

        #region POST
        //Metodo per permettere allo studente di iscriversi ad un corso
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpPost("CourseRegistration/{courseId}")]
        public async Task<IActionResult> AddToStudyPlan(string courseId)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
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
        [Authorize(AuthenticationSchemes = "StudentScheme")]
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

        private bool StudentCourseExists(int id)
        {
            return _context.StudentCourses.Any(e => e.Id == id);
        }
    }
}

public class CourseInfos
{
    public int Id { get; set; }
    public string CourseId { get; set; }

    public string Name { get; set; }

    public string? ProfFullName { get; set; }

    public string? Classrooms { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

}
