namespace PetDoa.DTOs
{
    namespace PetDoa.DTOs.Administrator
    {
        public class CreateAdministratorDTO
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password_Hash { get; set; }
            public DateTime Registration_Date { get; set; }
            public int Ong_ID { get; set; }
        }
    }

}
