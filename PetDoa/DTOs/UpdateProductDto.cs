using System.ComponentModel.DataAnnotations;

namespace PetDoa.DTOs
{
  public class UpdateProductDto
  {
    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
    public string Name { get; set; }

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0.01, 10000.00, ErrorMessage = "O preço deve ser entre R$0,01 e R$10.000,00.")]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; }

    public bool IsActive { get; set; }
  }
}
