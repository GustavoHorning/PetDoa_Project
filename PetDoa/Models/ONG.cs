using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PetDoa.Models
{
    [Table("ONG")]
    public class ONG
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        [Column("RegistrationDate")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [JsonIgnore]  // Ignora a serialização da lista de Administradores
        public List<Administrator> Administrators { get; set; }
        public List<Donation> Donations { get; set; }
    }

}
