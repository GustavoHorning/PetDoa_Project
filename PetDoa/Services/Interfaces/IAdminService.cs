using PetDoa.DTOs;

namespace PetDoa.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminLoginResponseDTO?> AuthenticateAsync(AdminLoginDTO dto);

        Task<IEnumerable<AdminReadDTO>> GetAdministratorsAsync();

        Task<AdminReadDTO?> GetAdminByIdAsync(int id);

        Task<AdminReadDTO> CreateAdminAsync(CreateAdminApiDTO createDto); // Usando o novo DTO

        Task<bool> DeleteAdminAsync(int id);
    }
}