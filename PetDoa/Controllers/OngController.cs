using Microsoft.AspNetCore.Mvc;
using PetDoa.Services.Interfaces; 
using PetDoa.DTOs;
namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OngController : ControllerBase
    {

        private readonly IOngService _ongService;
        public OngController(IOngService ongService)
        {
            _ongService = ongService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOng([FromBody] CreateOngDTO dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdOngDto = await _ongService.CreateOngAsync(dto);

            return CreatedAtAction(nameof(GetOngById), new { id = createdOngDto.Id }, createdOngDto);
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OngReadDTO>>> GetAll()
        {
            var ongs = await _ongService.GetAllOngsAsync();
            return Ok(ongs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OngReadDTO>> GetOngById(int id)
        {
            var ongDto = await _ongService.GetOngByIdAsync(id);

            if (ongDto == null)
            {
                return NotFound();
            }

            return Ok(ongDto); 
        }

      
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOng(int id)
        {
            var deleted = await _ongService.DeleteOngAsync(id);

            if (!deleted)
            {
                return NotFound(); 
            }

            return NoContent(); 
        }
    }
}