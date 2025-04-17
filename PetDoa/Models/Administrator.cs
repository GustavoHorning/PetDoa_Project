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

        public int ONG_ID { get; set; }  // Chave estrangeira
        [ForeignKey("ONG_ID")]
        public ONG ONG { get; set; }  // Navegação para ONG
    }

}
