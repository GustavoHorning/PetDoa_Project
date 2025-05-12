using System.ComponentModel.DataAnnotations;

namespace PetDoa.DTOs
{
    public class CreateAdminApiDTO
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        public string Password { get; set; } 

        [Required(ErrorMessage = "O ID da ONG é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID da ONG inválido.")]
        public int OngId { get; set; }

    }
}