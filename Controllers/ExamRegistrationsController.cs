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
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpGet("ExamsByStudent")]
        public async Task<ActionResult<IEnumerable<ExamInfos>>> GetSelectedExams()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var selectedExams = await _context.ExamRegistrations
                .Where(s => s.StudId == studID)
                .Include(e => e.Exam)
                    .ThenInclude(pc => pc.ProfCourseExams)
                    .ThenInclude(p => p.Prof)
                .Select(e => new ExamInfos
                {
                    Id = e.Id,
                    ExamId = e.Exam.ExamId,
                    Name = e.Exam.Name,
                    Cfu = e.Exam.Cfu,
                    Type = e.Exam.Type,
                    CourseId = e.Exam.CourseId,
                    ProfFullName = e.Exam.ProfCourseExams.FirstOrDefault() != null
                        ? e.Exam.ProfCourseExams.FirstOrDefault()!.Prof.Name
                        + " " + e.Exam.ProfCourseExams.FirstOrDefault()!.Prof.Surname
                        : null,
                    Date = e.Exam.Date
                }).ToListAsync();

            if (selectedExams == null)
            {
                return NotFound("Nessun esame selezionato");
            }

            return Ok(selectedExams);
        }

        //Metodo per prendere esami in programma (futuri) di uno studente
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpGet("FutureExams")]
        public async Task<ActionResult<IEnumerable<ExamInfos>>> GetFutureExams()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var exams = await _context.ExamRegistrations
                .Where(s => s.StudId == studID)
                .Include(e => e.Exam)
                    .ThenInclude(pc => pc.ProfCourseExams)
                    .ThenInclude(p => p.Prof)
                .Select(e => new ExamInfos
                {
                    ExamId = e.Exam.ExamId,
                    Name = e.Exam.Name,
                    Cfu = e.Exam.Cfu,
                    Type = e.Exam.Type,
                    CourseId = e.Exam.CourseId,
                    ProfFullName = e.Exam.ProfCourseExams.FirstOrDefault() != null
                        ? e.Exam.ProfCourseExams.FirstOrDefault()!.Prof.Name
                        + " " + e.Exam.ProfCourseExams.FirstOrDefault()!.Prof.Surname
                        : null,
                    Date = e.Exam.Date
                }).ToListAsync();

            if (exams == null)
            {
                return NotFound("Nessun esame selezionato");
            }

            return Ok(exams);
        }
        #endregion

        #region POST
        //Metodo per permettere allo studente di iscriversi ad un esame
        [Authorize(AuthenticationSchemes = "StudentScheme")]
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
        [Authorize(AuthenticationSchemes = "StudentScheme")]
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
        #endregion

        private bool ExamRegistrationExists(int id)
        {
            return _context.ExamRegistrations.Any(e => e.Id == id);
        }
    }
}