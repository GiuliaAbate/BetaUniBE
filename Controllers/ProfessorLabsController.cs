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
    public class ProfessorLabsController : ControllerBase
    {
        private readonly IubContext _context;

        public ProfessorLabsController(IubContext context)
        {
            _context = context;
        }

        // GET: api/ProfessorLabs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfessorLab>>> GetProfessorLabs()
        {
            return await _context.ProfessorLabs.ToListAsync();
        }

        // GET: api/ProfessorLabs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfessorLab>> GetProfessorLab(int id)
        {
            var professorLab = await _context.ProfessorLabs.FindAsync(id);

            if (professorLab == null)
            {
                return NotFound();
            }

            return professorLab;
        }

        // PUT: api/ProfessorLabs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfessorLab(int id, ProfessorLab professorLab)
        {
            if (id != professorLab.Id)
            {
                return BadRequest();
            }

            _context.Entry(professorLab).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfessorLabExists(id))
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

        // POST: api/ProfessorLabs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProfessorLab>> PostProfessorLab(ProfessorLab professorLab)
        {
            _context.ProfessorLabs.Add(professorLab);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProfessorLab", new { id = professorLab.Id }, professorLab);
        }

        // DELETE: api/ProfessorLabs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessorLab(int id)
        {
            var professorLab = await _context.ProfessorLabs.FindAsync(id);
            if (professorLab == null)
            {
                return NotFound();
            }

            _context.ProfessorLabs.Remove(professorLab);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfessorLabExists(int id)
        {
            return _context.ProfessorLabs.Any(e => e.Id == id);
        }

        #region POST
        //Metodo per permettere al prof di scegliere un laboratorio
        [HttpPost("ProfAddLab/{labId}")]
        public async Task<IActionResult> AddLab(int labId)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var lab = await _context.Laboratories.FindAsync(labId);
            if (lab == null)
            {
                return NotFound("Laboratorio non trovato");
            }

            bool alreadyRegistered = await _context.ProfessorLabs
                .AnyAsync(p => p.ProfId.Equals(profID) && p.LabId == labId);
            if (alreadyRegistered)
            {
                return BadRequest("Laboratorio già scelto");
            }

            var chosenLab = new ProfessorLab
            {
                ProfId = profID,
                LabId = labId,
                DepartmentId = lab.DepartmentId
            };

            _context.ProfessorLabs.Add(chosenLab);
            await _context.SaveChangesAsync();

            return Ok(chosenLab);
        }
        #endregion

        #region DELETE
        //Rimuovere da un laboratorio
        [HttpDelete("DeleteLab/{id}")]
        public async Task<IActionResult> DeleteLabRegistration(int id)
        {
            var profID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(profID))
            {
                return Unauthorized("Utente non autenticato");
            }

            var labRegistration = await _context.ProfessorLabs.FindAsync(id);
            if (labRegistration == null)
            {
                return NotFound();
            }

            _context.ProfessorLabs.Remove(labRegistration);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion
    }
}
