using PetDoa.DTOs;

namespace PetDoa.Services.Interfaces
{
    public interface IOngService
    {
        Task<IEnumerable<OngReadDTO>> GetAllOngsAsync();

        Task<OngReadDTO?> GetOngByIdAsync(int id);

        Task<OngReadDTO> CreateOngAsync(CreateOngDTO createDto);

        Task<bool> DeleteOngAsync(int id);
    }
}