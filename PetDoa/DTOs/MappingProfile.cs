using AutoMapper;
using PetDoa.DTOs;
using PetDoa.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<Administrator, AdminReadDTO>();

        CreateMap<ONG, OngBasicDTO>();
        CreateMap<ONG, OngReadDTO>();

        CreateMap<Donation, DonationReadDTO>();
        CreateMap<Donor, DonorBasicDTO>();
        CreateMap<CreateDonationDTO, Donation>();
        CreateMap<Donor, DonorReadDTO>();

    }
}
