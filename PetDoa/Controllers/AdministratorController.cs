using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetDoa.DTOs; 
using PetDoa.Services.Interfaces;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdministratorController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdministratorController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<ActionResult<AdminReadDTO>> CreateAdmin([FromBody] CreateAdminApiDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdAdminDto = await _adminService.CreateAdminAsync(dto);

                return CreatedAtAction(nameof(GetAdminById), new { id = createdAdminDto.ID }, createdAdminDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminReadDTO>>> GetAdministrators()
        {
            var admins = await _adminService.GetAdministratorsAsync();
            return Ok(admins);
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminReadDTO>> GetAdminById(int id)
        {
            var adminDto = await _adminService.GetAdminByIdAsync(id);

            if (adminDto == null)
            {
                return NotFound();
            }

            return Ok(adminDto);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var deleted = await _adminService.DeleteAdminAsync(id);

            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }

        
    }
}