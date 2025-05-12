using System.ComponentModel.DataAnnotations;

namespace PetDoa.DTOs
{
  public class ChangePasswordDTO
  {
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A nova senha deve ter pelo menos 8 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "A confirmação da nova senha é obrigatória.")]
    [Compare("NewPassword", ErrorMessage = "A nova senha e a confirmação não coincidem.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
  }
}
