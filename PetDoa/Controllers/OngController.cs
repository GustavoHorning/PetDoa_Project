using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.DTOs.PetDoa.DTOs.Ong;
using PetDoa.Models;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OngController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;


        public OngController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // OngController
        [HttpPost]
        public async Task<IActionResult> CreateOng([FromBody] CreateOngDTO dto)
        {
            var ong = new ONG
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Description = dto.Description,
                RegistrationDate = DateTime.Now
            };

            _context.ONGs.Add(ong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOngById), new { id = ong.ID }, ong);
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<OngReadDTO>>> GetAll()
        {
            var ongs = await _context.ONGs
                .Include(o => o.Administrators) // NÃO dar ThenInclude aqui
                .ToListAsync();

            var dto = _mapper.Map<List<OngReadDTO>>(ongs);
            return Ok(dto);
        }






        [HttpGet("{id}")]
        public async Task<ActionResult<OngReadDTO>> GetOngById(int id)
        {
            var ong = await _context.ONGs
                .Include(o => o.Administrators)
                .FirstOrDefaultAsync(o => o.ID == id);

            if (ong == null)
            {
                return NotFound();
            }

            var ongDTO = _mapper.Map<OngReadDTO>(ong);
            return Ok(ongDTO);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOng(int id, ONG ong)
        {
            if (id != ong.ID) return BadRequest();

            _context.Entry(ong).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOng(int id)
        {
            var ong = await _context.ONGs.FindAsync(id);
            if (ong == null) return NotFound();

            _context.ONGs.Remove(ong);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
