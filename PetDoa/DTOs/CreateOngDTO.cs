// PetDoa/DTOs/PetDoa.DTOs.Ong/CreateOngDTO.cs (Namespace um pouco estranho, talvez PetDoa.DTOs.Ong?)
using System.ComponentModel.DataAnnotations;

namespace PetDoa.DTOs // Ou PetDoa.DTOs.Ong
{
    public class CreateOngDTO
    {
        [Required(ErrorMessage = "O nome da ONG é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail da ONG é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(256, ErrorMessage = "O e-mail não pode exceder 256 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone da ONG é obrigatório.")]
        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        [StringLength(20, ErrorMessage = "O telefone não pode exceder 20 caracteres.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "A descrição da ONG é obrigatória.")]
        [StringLength(1000, ErrorMessage = "A descrição não pode exceder 1000 caracteres.")]
        public string Description { get; set; }

    }
}