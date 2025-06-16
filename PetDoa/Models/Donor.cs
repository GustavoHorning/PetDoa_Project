using System.ComponentModel.DataAnnotations.Schema;

namespace PetDoa.Models
{
    [Table("Donor")]

    public class Donor
    {
        public int ID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Password_Hash { get; set; }

        public string? OAuth_Provider { get; set; }

        public string? OAuth_ID { get; set; }

        [Column("Registration_Date")]
        public DateTime Registration_Date { get; set; } = DateTime.Now;
        public string? ProfilePictureUrl { get; set; }

    public virtual ICollection<Donation> Donations { get; set; }



  }
}
