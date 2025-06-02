namespace PetDoa.DTOs
{
    public class AdminReadDTO
    {
        
            public int ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public DateTime Registration_Date { get; set; }
            public int OngId { get; set; } // só o ID, não o objeto inteiro
            public bool IsSuperAdmin { get; set; }


    }
}


