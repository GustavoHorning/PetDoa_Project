// PetDoa/Dtos/Donor/RegisterDonorDto.cs (Verifique o namespace)
using System.ComponentModel.DataAnnotations;

namespace PetDoa.DTOs // Ou o namespace correto
{
    public class RegisterDonorDto
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(256, ErrorMessage = "O e-mail não pode exceder 256 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não coincidem.")] // Compara com a propriedade Password
        public string ConfirmPassword { get; set; }
    }
}