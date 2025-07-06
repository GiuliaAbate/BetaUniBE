using BetaUni.Models;
using BetaUni.Other;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BetaUni.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IubContext _context;
        private readonly Services _services;

        public StudentsController(IubContext context, Services services)
        {
            _context = context;
            _services = services;
        }


        // PUT: api/Students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(string id, Student student)
        {
            if (id != student.StudId)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        // POST: api/Students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            _context.Students.Add(student);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StudentExists(student.StudId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetStudent", new { id = student.StudId }, student);
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudId == id);
        }

        #region GET
        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.ToListAsync();
        }


        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(string id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        //Metodo con la quale lo studente potrà vedere i suoi dati, quindi si prende il token per capire chi è l'utente
        [HttpGet("ViewStudentInfo")]
        public async Task<IActionResult> GetStudentInfo()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var student = await _context.Students.FindAsync(studID);

            return Ok(student);
        }

        #endregion

        #region POST

        //Registrazione dello studente
        [HttpPost("StudRegistration")]
        public async Task<ActionResult<Student>> SignStudent(StudentRegistration student)
        {
            try
            {
                var existingStud = await _context.Students
                 .Where(s => s.Email == student.Email)
                 .FirstOrDefaultAsync();

                if (existingStud != null)
                {
                    return BadRequest("Email già registrata.");
                }
                else
                {
                    var encrypted = Services.SaltEncryption(student.Password);
                    Student stud = new Student
                    {
                        StudId = _services.GenerateStudId(),
                        Name = student.Name,
                        Surname = student.Surname,
                        BirthDate = student.BirthDate,
                        Email = student.Email,
                        Password = encrypted.Key,
                        Salt = encrypted.Value,
                        PhoneNumber = student.PhoneNumber,
                        DepartmentId = student.DepartmentId,
                        EnrollmentDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _context.Students.Add(stud);
                    await _context.SaveChangesAsync();

                    return Ok(stud);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #endregion

        #region PUT
        [HttpPut("UpdateStudent")]
        public async Task<IActionResult> UpdateStudInfos(StudInfos studInfos)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var existingStudent = await _context.Students.FindAsync(studID);
            if (existingStudent == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(studInfos.PhoneNumber) &&
                studInfos.PhoneNumber != existingStudent.PhoneNumber)
            {
                existingStudent.PhoneNumber = studInfos.PhoneNumber;
            }

            // Si cripta di nuovo la password quando si va a cambiare
            if (!string.IsNullOrEmpty(studInfos.Password))
            {
                var encrypted = Services.SaltEncryption(studInfos.Password);
                existingStudent.Password = encrypted.Key;
                existingStudent.Salt = encrypted.Value;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        #endregion
    }
}

public class StudInfos
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
}

public class StudentRegistration
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Surname { get; set; }

    [Required]
    public DateOnly BirthDate { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string DepartmentId { get; set; }
}