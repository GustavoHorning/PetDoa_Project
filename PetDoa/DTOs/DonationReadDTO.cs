using PetDoa.Dtos.Ong;
using PetDoa.DTOs;

public class DonationReadDTO
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime Date { get; set; }

    public DonorReadDTO? Donor { get; set; }
    public OngBasicDTO ONG { get; set; }
}


