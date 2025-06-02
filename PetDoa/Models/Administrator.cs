using System.ComponentModel.DataAnnotations.Schema;

namespace PetDoa.Models
{
   
        [Table("Administrator")]
        public class Administrator
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password_Hash { get; set; }
            public DateTime Registration_Date { get; set; } = DateTime.Now;
            public int OngId { get; set; }  // Somente o ID da ONG
            public bool IsSuperAdmin { get; set; }
            [ForeignKey("OngId")]
            public ONG ONG { get; set; }

    }
}
