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

        public DateTime Date { get; set; } = DateTime.UtcNow;


    [Column("DonorID")]
        public int? DonorID { get; set; }

        public Donor? Donor { get; set; }

        
        public int OngId { get; set; }

        [ForeignKey("OngId")]
        public ONG? ONG { get; set; }

        public DonationStatus Status { get; set; } 
        public string? GatewayPaymentId { get; set; }

    }
}
