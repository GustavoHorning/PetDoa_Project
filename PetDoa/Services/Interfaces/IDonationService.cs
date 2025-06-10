// PetDoa/Services/Interfaces/IDonationService.cs
using PetDoa.DTOs;
using static PetDoa.Controllers.DonationController; // Namespace para DonationReadDTO, CreateDonationDTO

namespace PetDoa.Services.Interfaces
{
    public interface IDonationService
    {
        
        Task<DonationReadDTO> CreateDonationAsync(CreateDonationDTO createDto, int donorId);

        Task<IEnumerable<DonationReadDTO>> GetAllDonationsAsync();

        Task<DonationReadDTO?> GetDonationByIdAsync(int id);

        Task<IEnumerable<DonationReadDTO>> GetDonationsByDonorAsync(int donorId);

        Task<bool> DeleteDonationAsync(int id);

        Task<DashboardSummaryDto> GetDonationSummaryByDonorAsync(int donorId);

  }
}
