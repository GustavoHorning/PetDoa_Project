using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetDoa.DTOs;
using PetDoa.Services.Interfaces;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        public class DashboardSummaryDto
        {
          public decimal TotalDonated { get; set; }
          public DateTime? LastDonationDate { get; set; }
          public string DonorName { get; set; }
        }

        [Authorize] 
        [HttpPost]
        public async Task<ActionResult<DonationReadDTO>> CreateDonation(CreateDonationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
            {
                return Unauthorized("Não foi possível identificar o usuário.");
            }

            try
            {
                var createdDonationDto = await _donationService.CreateDonationAsync(dto, donorId);

                return CreatedAtAction(nameof(GetDonation), new { id = createdDonationDto.Id }, createdDonationDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetAllDonations()
        {
            var donations = await _donationService.GetAllDonationsAsync();
            return Ok(donations);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DonationReadDTO>> GetDonation(int id)
        {
            var donationDto = await _donationService.GetDonationByIdAsync(id);

            if (donationDto == null)
            {
                return NotFound();
            }

            return Ok(donationDto);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetMyDonations()
        {
          var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
            {
                return Unauthorized("Não foi possível identificar o usuário.");
            }

            var donations = await _donationService.GetDonationsByDonorAsync(donorId);

            return Ok(donations);
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonation(int id)
        {
            var deleted = await _donationService.DeleteDonationAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }


        [Authorize]
        [HttpGet("my-summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetMyDonationSummary()
        {
          var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
          if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
          {
            return Unauthorized("Não foi possível identificar o usuário.");
          }

          var summary = await _donationService.GetDonationSummaryByDonorAsync(donorId);

          if (summary == null)
          {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            return Ok(new DashboardSummaryDto { TotalDonated = 0, DonorName = userName });
          }

          return Ok(summary);
        }
    }
}
