using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.Models;

namespace PetDoa.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize]
  public class ProductsController : ControllerBase
  {
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetActiveProducts()
    {
      var products = await _context.Products.Where(p => p.IsActive).ToListAsync();
      return Ok(products);
    }


    [HttpGet("all")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProductsForAdmin()
    {
      var products = await _context.Products.OrderByDescending(p => p.Id).ToListAsync();
      return Ok(products);
    }


    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var product = new Product
      {
        Name = createProductDto.Name,
        Description = createProductDto.Description,
        Price = createProductDto.Price,
        ImageUrl = createProductDto.ImageUrl,
        IsActive = true
      };

      _context.Products.Add(product);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetActiveProducts), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
      var product = await _context.Products.FindAsync(id);

      if (product == null)
      {
        return NotFound();
      }

      product.Name = updateProductDto.Name;
      product.Description = updateProductDto.Description;
      product.Price = updateProductDto.Price;
      product.ImageUrl = updateProductDto.ImageUrl;
      product.IsActive = updateProductDto.IsActive;

      await _context.SaveChangesAsync();

      return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeactivateProduct(int id)
    {
      var product = await _context.Products.FindAsync(id);
      if (product == null)
      {
        return NotFound();
      }

      product.IsActive = false;
      await _context.SaveChangesAsync();

      return NoContent();
    }



  }
}
