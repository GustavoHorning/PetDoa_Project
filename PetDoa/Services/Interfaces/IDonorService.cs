using PetDoa.DTOs;
using PetDoa.Models;

namespace PetDoa.Services.Interfaces
{
    public interface IDonorService
    {
        Task<ServiceResponse<Donor>> RegisterDonorAsync(RegisterDonorDto dto);

        Task<string?> LoginDonorAsync(LoginDonorDto dto); 

        Task<DonorReadDTO?> GetDonorByIdAsync(int id); 


        Task<IEnumerable<DonorReadDTO>> GetAllDonorsAsync(); 

        Task<bool> DeleteDonorAsync(int id);

        Task<string?> GenerateJwtForDonorAsync(Donor donor);
        Task<OAuthSignInResult> FindOrCreateDonorByOAuthAsync(string provider, string providerKey, string email, string? name); 
        Task<DonorReadDTO?> GetDonorProfileAsync(int donorId);
        Task<bool> UpdateDonorProfileAsync(int donorId, UpdateDonorProfileDTO dto);
        Task<ServiceResponse<object?>> ChangePasswordAsync(int donorId, ChangePasswordDTO dto);
        Task<string?> UpdateProfilePictureAsync(int donorId, IFormFile imageFile);
        Task<bool> DeleteProfilePictureAsync(int donorId);


  }
}
