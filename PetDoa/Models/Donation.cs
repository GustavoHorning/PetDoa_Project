using System.ComponentModel.DataAnnotations.Schema;
using PetDoa.Models.Enums;

namespace PetDoa.Models
{
    [Table("Donation")]
    public class Donation
    {
        public int ID { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }

        public bool IsRecurring { get; set; } = false;

        public DateTime Date { get; set; } = DateTime.Now;

        // FK: Doação anônima (Donor pode ser null)
        [Column("DonorID")]
        public int? DonorID { get; set; }

        public Donor? Donor { get; set; }

        
        public int OngId { get; set; }

        [ForeignKey("OngId")]
        public ONG? ONG { get; set; }
    }
}
