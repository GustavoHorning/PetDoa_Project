using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.Models;
using PetDoa.Models.Enums;
using PetDoa.Services.Interfaces;
using PetDoa.Services.PdfGenerator;
using QuestPDF.Fluent;
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

    //public async Task<IEnumerable<DonationReadDTO>> GetDonationsByDonorAsync(int donorId)
    //{
    //    var donations = await _context.Donations
    //                                  .Where(d => d.DonorID == donorId && d.Status == DonationStatus.Completed)
    //                                  .OrderByDescending(d => d.Date)
    //                                  .ToListAsync();

    //    return _mapper.Map<List<DonationReadDTO>>(donations);
    //}

    public async Task<PaginatedResultDto<DonationReadDTO>> GetDonationsByDonorAsync(int donorId, int pageNumber, int pageSize)
    {
      var query = _context.Donations
          .Where(d => d.DonorID == donorId && d.Status == DonationStatus.Completed)
          .OrderByDescending(d => d.Date);

      var totalCount = await query.CountAsync();

      var items = await query
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

      var dtos = _mapper.Map<List<DonationReadDTO>>(items);

      return new PaginatedResultDto<DonationReadDTO>(dtos, totalCount, pageNumber, pageSize);
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
          var itemsDonatedCount = await _context.Donations
            .CountAsync(d => d.DonorID == donorId &&
                     d.Status == DonationStatus.Completed &&
                     d.ProductId != null);

      return new DashboardSummaryDto
          {
            TotalDonated = totalAmount,
            LastDonationDate = lastDonationDate,
            DonorName = donor.Name,
            ItemsDonatedCount = itemsDonatedCount
          };
        }

    public async Task<byte[]> GenerateReceiptPdfAsync(int donationId, int donorId)
    {
      try
      {
        var donation = await _context.Donations
            .Include(d => d.Donor)
            .Include(d => d.ONG)
            .FirstOrDefaultAsync(d => d.ID == donationId);

        if (donation == null || donation.DonorID != donorId)
        {
          throw new UnauthorizedAccessException("Recibo não encontrado ou pertence a outro usuário.");
        }

        var receiptDocument = new DonationReceipt(donation);
        byte[] pdfBytes = receiptDocument.GeneratePdf();

        return pdfBytes;
      }
      catch (Exception ex)
      {
        throw;
      }
    }


    public async Task<byte[]> GenerateConsolidatedReportPdfAsync(int donorId, DateTime startDate, DateTime endDate)
    {
      var inclusiveEndDate = endDate.AddDays(1);
      var donationsInPeriod = await _context.Donations
          .Include(d => d.ONG) 
          .Where(d => d.DonorID == donorId &&
                       d.Status == DonationStatus.Completed &&
                       d.Date >= startDate && d.Date < inclusiveEndDate)
          .OrderBy(d => d.Date)
          .ToListAsync();

      var donor = await _context.Donors.FindAsync(donorId);

      var reportDocument = new ConsolidatedReceipt(donor, startDate, endDate, donationsInPeriod);

      byte[] pdfBytes = reportDocument.GeneratePdf();
      return pdfBytes;
    }

    public async Task<IEnumerable<DonationReadDTO>> GetRecentDonationsByDonorAsync(int donorId, int limit = 3)
    {
      var recentDonations = await _context.Donations
          .Where(d => d.DonorID == donorId && d.Status == DonationStatus.Completed)
          .OrderByDescending(d => d.Date)
          .Take(limit)
          .ToListAsync();

      return _mapper.Map<List<DonationReadDTO>>(recentDonations);
    }

  }
}
