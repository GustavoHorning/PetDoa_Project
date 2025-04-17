using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.Models;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;


        public DonationController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // DonationController
        [HttpPost]
        public async Task<ActionResult<DonationReadDTO>> CreateDonation(CreateDonationDTO dto)
        {
            var donation = new Donation
            {
                Amount = dto.Amount,
                Method = dto.Method,
                IsRecurring = dto.IsRecurring,
                Date = dto.Date,
                DonorID = dto.DonorID,
                ONG_ID = dto.OngID
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // 🔥 Aqui você recarrega a doação com o Donor e a ONG incluídos
            var donationWithIncludes = await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.ONG)
                .FirstOrDefaultAsync(d => d.ID == donation.ID);

            var donationDTO = _mapper.Map<DonationReadDTO>(donationWithIncludes);
            return CreatedAtAction(nameof(GetDonation), new { id = donation.ID }, donationDTO);
        }





        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetAllDonations()
        {
            var donations = await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.ONG)
                .ToListAsync();

            var dto = _mapper.Map<List<DonationReadDTO>>(donations);
            return Ok(dto);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<DonationReadDTO>> GetDonation(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.ONG)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (donation == null)
            {
                return NotFound();
            }

            var donationDto = _mapper.Map<DonationReadDTO>(donation);
            return Ok(donationDto);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonation(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null) return NotFound();

            _context.Donations.Remove(donation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
