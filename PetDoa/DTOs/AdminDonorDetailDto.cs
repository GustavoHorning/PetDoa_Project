namespace PetDoa.DTOs
{
  public class AdminDonorDetailDto : AdminDonorListDto
  {
    public List<DonationReadDTO> DonationHistory { get; set; }
  }
}
