using BetaUni.Models;
using BetaUni.Other;
using Microsoft.AspNetCore.Authorization;
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

        //Metodo con la quale lo studente potrà vedere i suoi dati,
        //quindi si prende il token per capire chi è l'utente
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpGet("ViewStudentInfo")]
        public async Task<IActionResult> GetStudentInfo()
        {
            //Si trova utente se questo è connesso con i Claims
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Utente non autenticato");
            }

            //Nella variabile si vanno a prendere tutte le info necessarie
            var student = await _context.Students
                .Include(sd => sd.Department)
                .Where(s => s.StudId == studID)
                .Select(s => new
                {
                    s.StudId,
                    s.Name,
                    s.Surname,
                    s.BirthDate,
                    s.Email,
                    s.PhoneNumber,
                    s.DepartmentId,
                    s.EnrollmentDate,
                    DepartmentName = s.Department.Name
                }).FirstOrDefaultAsync();
                

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
                    //Nella variabile si memorizza la criptazione della password dello studente
                    var encrypted = Services.SaltEncryption(student.Password);
                    Student stud = new Student
                    {
                        StudId = _services.GenerateStudId(),
                        Name = student.Name,
                        Surname = student.Surname,
                        BirthDate = student.BirthDate,
                        Email = student.Email,
                        Password = encrypted.Key, //qui si memorizza solo la password criptata in hash256
                        Salt = encrypted.Value, //qui si memorizza il salt
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
        //Chiamata per permettere allo studente di aggiornare numero di telefono o password
        [Authorize(AuthenticationSchemes = "StudentScheme")]
        [HttpPut("UpdateStudent")]
        public async Task<IActionResult> UpdateStudInfos(StudInfos studInfos)
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studID))
            {
                return Unauthorized("Studente non autenticato");
            }

            var existingStudent = await _context.Students.FindAsync(studID);
            if (existingStudent == null)
            {
                return NotFound("Studente non trovato");
            }

            //Si controlla che si sia inserito il numero di telefono e che sia diverso da quello precedente
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


        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudId == id);
        }


    }
}

//Classe che contiene sono i dati che si possono cambiare
public class StudInfos
{
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
}

//Classe che contiene i dati richiesti per la registrazione
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