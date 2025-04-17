using PetDoa.Dtos.Donor;
using PetDoa.DTOs;
using PetDoa.Models;
using PetDoa.Util;

namespace PetDoa.Services.Interfaces
{
    public interface IDonorService
    {
        Task<ServiceResponse<Donor>> RegisterDonorAsync(RegisterDonorDto dto);
        Task<string?> LoginDonorAsync(LoginDonorDto dto);

    }
}
