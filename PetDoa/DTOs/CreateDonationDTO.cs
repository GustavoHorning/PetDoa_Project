namespace PetDoa.DTOs
{
    public class CreateDonationDTO
    {
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime Date { get; set; }

        public int DonorID { get; set; }
        public int OngID { get; set; }
    }

}
