// PetDoa/Services/DonationService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.Models;
using PetDoa.Models.Enums;
using PetDoa.Services.Interfaces;
using static PetDoa.Controllers.DonationController;

namespace PetDoa.Services
{
    public class DonationService : IDonationService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private const decimal MinDonationAmount = 0.01M;

        public DonationService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DonationReadDTO> CreateDonationAsync(CreateDonationDTO createDto, int donorId)
        {
            if (createDto.Amount < MinDonationAmount)
            {
                throw new ArgumentException($"O valor mínimo para doação é R$ {MinDonationAmount}.");
            }

            var ongExists = await _context.ONGs.AnyAsync(o => o.ID == createDto.OngID);
            if (!ongExists)
            {
                throw new ArgumentException($"ONG com ID {createDto.OngID} não encontrada.");
            }

            var donorExists = await _context.Donors.AnyAsync(d => d.ID == donorId);
            if (!donorExists)
            {
                throw new ArgumentException($"Doador com ID {donorId} não encontrado.");
            }

            var donation = new Donation
            {
                Amount = createDto.Amount,
                Method = createDto.Method,
                IsRecurring = createDto.IsRecurring,
                Date = DateTime.UtcNow,
                DonorID = donorId,
                OngId = createDto.OngID
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();
            return _mapper.Map<DonationReadDTO>(donation);
        }

        public async Task<IEnumerable<DonationReadDTO>> GetAllDonationsAsync()
        {
            var donations = await _context.Donations
                                          .ToListAsync();
            return _mapper.Map<List<DonationReadDTO>>(donations);
        }

        public async Task<DonationReadDTO?> GetDonationByIdAsync(int id)
        {
            var donation = await _context.Donations
                                         .FirstOrDefaultAsync(d => d.ID == id);

            return _mapper.Map<DonationReadDTO?>(donation);
        }

        public async Task<IEnumerable<DonationReadDTO>> GetDonationsByDonorAsync(int donorId)
        {
            var donations = await _context.Donations
                                          .Where(d => d.DonorID == donorId && d.Status == DonationStatus.Completed)
                                          .OrderByDescending(d => d.Date)
                                          .ToListAsync();

            return _mapper.Map<List<DonationReadDTO>>(donations);
        }

        public async Task<bool> DeleteDonationAsync(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
            {
                return false;
            }

            _context.Donations.Remove(donation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DashboardSummaryDto> GetDonationSummaryByDonorAsync(int donorId)
        {
          var donor = await _context.Donors.FindAsync(donorId);
          if (donor == null)
          {
            return null;
          }
          var completedDonationsQuery = _context.Donations
              .Where(d => d.DonorID == donorId && d.Status == DonationStatus.Completed);

          if (!await completedDonationsQuery.AnyAsync())
          {
            return new DashboardSummaryDto
            {
              TotalDonated = 0,
              LastDonationDate = null,
              DonorName = donor.Name
            };
          }

          var totalAmount = await completedDonationsQuery.SumAsync(d => d.Amount);
          var lastDonationDate = await completedDonationsQuery.MaxAsync(d => d.Date);

          return new DashboardSummaryDto
          {
            TotalDonated = totalAmount,
            LastDonationDate = lastDonationDate,
            DonorName = donor.Name
          };
        }
    }
}
