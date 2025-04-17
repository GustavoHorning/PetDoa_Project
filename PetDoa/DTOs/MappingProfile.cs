using AutoMapper;
using PetDoa.Dtos.Administrator;
using PetDoa.Dtos.Ong;
using PetDoa.DTOs;
using PetDoa.DTOs.PetDoa.DTOs.Administrator;
using PetDoa.DTOs.PetDoa.DTOs.Ong;
using PetDoa.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
      

        //CreateMap<CreateOngDTO, ONG>();
        //CreateMap<CreateAdministratorDTO, Administrator>();
        
        CreateMap<Administrator, AdminReadDTO>()
    .ForMember(dest => dest.ONG, opt => opt.MapFrom(src => src.ONG));

        CreateMap<ONG, OngBasicDTO>();
        CreateMap<ONG, OngReadDTO>();
        //CreateMap<Administrator, AdminReadDTO>();

        CreateMap<Donation, DonationReadDTO>();
        CreateMap<Donor, DonorBasicDTO>();
        CreateMap<CreateDonationDTO, Donation>();
        CreateMap<Donor, DonorReadDTO>();




    }
}
