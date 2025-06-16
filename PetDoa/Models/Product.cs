using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PetDoa.Models
{
  public class Product
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } 

    public bool IsActive { get; set; } = true; 
  }
}
