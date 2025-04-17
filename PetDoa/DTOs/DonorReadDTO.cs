namespace PetDoa.DTOs
{
    public class DonorReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? OAuth_Provider { get; set; }
        public string? OAuth_ID { get; set; }
        public DateTime Registration_Date { get; set; }
    }

}
