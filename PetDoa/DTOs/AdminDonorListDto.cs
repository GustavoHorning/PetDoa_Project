namespace PetDoa.DTOs
{
  public class AdminDonorListDto
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime RegistrationDate { get; set; }
    public int DonationCount { get; set; }
    public decimal TotalDonated { get; set; }
  }
}
