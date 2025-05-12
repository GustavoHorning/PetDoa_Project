// PetDoa/DTOs/AdminLoginDTO.cs
using System.ComponentModel.DataAnnotations;

namespace PetDoa.DTOs
{
    public class AdminLoginDTO
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Password { get; set; }
    }
}