using System.ComponentModel.DataAnnotations;

namespace PetDoa.DTOs
{
  public class UpdateDonorProfileDTO
  {
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    public string Name { get; set; } = string.Empty;
  }
}
