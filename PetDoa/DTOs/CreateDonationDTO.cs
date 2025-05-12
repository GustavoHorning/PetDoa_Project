// PetDoa/DTOs/CreateDonationDTO.cs
using System.ComponentModel.DataAnnotations;
using PetDoa.Models.Enums;

namespace PetDoa.DTOs
{
    public class CreateDonationDTO
    {
        [Required(ErrorMessage = "O valor da doação é obrigatório.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "O valor da doação deve ser positivo.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "O método de pagamento é obrigatório.")]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Método de pagamento inválido.")] 
        public PaymentMethod Method { get; set; } 
        public bool IsRecurring { get; set; }

        [Required(ErrorMessage = "O ID da ONG é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID da ONG inválido.")]
        public int OngID { get; set; }
    }
}