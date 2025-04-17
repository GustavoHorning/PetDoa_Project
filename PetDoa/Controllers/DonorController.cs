using Microsoft.AspNetCore.Mvc;
using PetDoa.Data; // Ajuste com o namespace correto do seu DbContext
using PetDoa.Models; // Ajuste com o namespace dos seus Models
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using PetDoa.DTOs;
using PetDoa.Services.Interfaces;
using PetDoa.Dtos.Donor;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonorController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDonorService _donorService;



        public DonorController(AppDbContext context, IMapper mapper, IDonorService donorService)
        {
            _context = context;
            _mapper = mapper;
            _donorService = donorService;
        }

        // 1. POST: Criar doador
        [HttpPost]
        public async Task<IActionResult> CreateDonor(Donor donor)
        {
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDonorById), new { id = donor.ID }, donor);
        }

        // 2. GET: Listar todos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Donor>>> GetAllDonors()
        {
            return await _context.Donors.ToListAsync();
        }

        // 3. GET: Buscar por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Donor>> GetDonorById(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
                return NotFound();

            return donor;
        }

        // 4. PUT: Atualizar
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDonor(int id, Donor donor)
        {
            if (id != donor.ID)
                return BadRequest();

            _context.Entry(donor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Donors.Any(e => e.ID == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // 5. DELETE: Remover
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonor(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
                return NotFound();

            _context.Donors.Remove(donor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDonorDto dto)
        {
            var result = await _donorService.RegisterDonorAsync(dto);

            if (!result.Success)
            {
                // Se não for bem-sucedido, retorna BadRequest com as mensagens de erro
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            // Se for bem-sucedido, retorna o status de sucesso
            return Ok(result.Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDonorDto dto)
        {
            var token = await _donorService.LoginDonorAsync(dto);

            if (token == null)
                return Unauthorized("E-mail ou senha inválidos.");

            return Ok(new { Token = token });
        }






    }
}
