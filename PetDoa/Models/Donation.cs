using System.ComponentModel.DataAnnotations.Schema;

namespace PetDoa.Models
{
    [Table("Donation")]
    public class Donation
    {
        public int ID { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } = string.Empty; // Pix, Cartão, Boleto etc.

        public bool IsRecurring { get; set; } = false;

        public DateTime Date { get; set; } = DateTime.Now;

        // FK: Doação anônima (Donor pode ser null)
        [Column("DonorID")]
        public int? DonorID { get; set; }

        public Donor? Donor { get; set; }

        
        public int ONG_ID { get; set; }

        [ForeignKey("ONG_ID")]
        public ONG? ONG { get; set; }
    }
}
