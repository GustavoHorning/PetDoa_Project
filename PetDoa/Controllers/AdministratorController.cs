using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.Dtos.Administrator;
using PetDoa.DTOs.PetDoa.DTOs.Administrator;
using PetDoa.Models;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdministratorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;


        public AdministratorController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // AdminController
        // POST: api/Admin
        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdministratorDTO dto)
        {
            // Verifica se a ONG existe
            var ong = await _context.ONGs.FindAsync(dto.Ong_ID);
            if (ong == null)
            {
                return BadRequest("ONG não encontrada.");
            }

            var admin = new Administrator
            {
                Name = dto.Name,
                Email = dto.Email,
                Password_Hash = dto.Password_Hash,
                ONG_ID = dto.Ong_ID,
                Registration_Date = DateTime.Now
            };

            _context.Administrators.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdminById), new { id = admin.ID }, admin);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminReadDTO>>> GetAdministrators()
        {
            var admins = await _context.Administrators
                .Include(a => a.ONG)
                .ToListAsync();

            return Ok(_mapper.Map<List<AdminReadDTO>>(admins));
        }




        // GET por ID para retorno do CreatedAtAction
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminReadDTO>> GetAdminById(int id)
        {
            var admin = await _context.Administrators
                .Include(a => a.ONG)
                .FirstOrDefaultAsync(a => a.ID == id);

            if (admin == null)
                return NotFound();

            var adminDTO = _mapper.Map<AdminReadDTO>(admin);
            return Ok(adminDTO);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, Administrator admin)
        {
            if (id != admin.ID) return BadRequest();

            _context.Entry(admin).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Administrators.FindAsync(id);
            if (admin == null) return NotFound();

            _context.Administrators.Remove(admin);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
