using PetDoa.Dtos.Ong;

namespace PetDoa.Dtos.Administrator
{
    public class AdminReadDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Registration_Date { get; set; }

        public OngBasicDTO ONG { get; set; }
    }
}


