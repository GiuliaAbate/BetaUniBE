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
    public class LaboratoriesController : ControllerBase
    {
        private readonly IubContext _context;

        public LaboratoriesController(IubContext context)
        {
            _context = context;
        }

        // GET: api/Laboratories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Laboratory>>> GetLaboratories()
        {
            return await _context.Laboratories.ToListAsync();
        }

        // GET: api/Laboratories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Laboratory>> GetLaboratory(int id)
        {
            var laboratory = await _context.Laboratories.FindAsync(id);

            if (laboratory == null)
            {
                return NotFound();
            }

            return laboratory;
        }

        [HttpGet("GetLabsByDep/{depID}")]
        public async Task<IActionResult> GetLabsByDep(string depID)
        {
            var laboratory = await _context.Laboratories
                .Where(c => c.DepartmentId == depID)
                .Select(c => new
                {
                    c.Name,
                    c.Attendance,
                }).ToListAsync();

            if (laboratory == null)
            {
                return NotFound("Nessun laboratorio trovato");
            }

            return Ok(laboratory);
        }

        [Authorize]
        [HttpGet("DepLabs")]
        public async Task<IActionResult> GetLabsFromDepartment()
        {
            var studID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var student = await _context.Students
                .Include(s => s.Department)
                .Where(s => s.StudId.Equals(studID))
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound("Utente non trovato");
            }

            var lab = await _context.Laboratories
                .Where(c => c.DepartmentId.Equals(student.DepartmentId))
                .ToListAsync();

            return Ok(lab);
        }


        // PUT: api/Laboratories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLaboratory(int id, Laboratory laboratory)
        {
            if (id != laboratory.LabId)
            {
                return BadRequest();
            }

            _context.Entry(laboratory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LaboratoryExists(id))
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

        // POST: api/Laboratories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Laboratory>> PostLaboratory(Laboratory laboratory)
        {
            _context.Laboratories.Add(laboratory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLaboratory", new { id = laboratory.LabId }, laboratory);
        }

        // DELETE: api/Laboratories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLaboratory(int id)
        {
            var laboratory = await _context.Laboratories.FindAsync(id);
            if (laboratory == null)
            {
                return NotFound();
            }

            _context.Laboratories.Remove(laboratory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LaboratoryExists(int id)
        {
            return _context.Laboratories.Any(e => e.LabId == id);
        }
    }
}
