using Microsoft.AspNetCore.Mvc;
using PetDoa.Services.Interfaces;
using PetDoa.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonorController : ControllerBase
    {
        private readonly IDonorService _donorService;
        public DonorController(IDonorService donorService)
        {
            _donorService = donorService;
        }

        [Authorize(Roles = "Admin,SuperAdmin")] 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonorReadDTO>>> GetAllDonors()
        {
            var donors = await _donorService.GetAllDonorsAsync();
            return Ok(donors);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DonorReadDTO>> GetDonorById(int id)
        {
            var donorDto = await _donorService.GetDonorByIdAsync(id);

            if (donorDto == null)
            {
                return NotFound();
            }

            return Ok(donorDto); 
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonor(int id)
        {
            var deleted = await _donorService.DeleteDonorAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }


        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<DonorReadDTO>> GetMyProfile()
        {
          var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

          if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
          {
            return Unauthorized(new { message = "Não foi possível identificar o usuário a partir do token." });
          }

          var donorProfile = await _donorService.GetDonorProfileAsync(donorId);

          if (donorProfile == null)
          {
            return NotFound(new { message = "Perfil do doador não encontrado." });
          }
          return Ok(donorProfile);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDonorProfileDTO dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
      {
        return Unauthorized(new { message = "Não foi possível identificar o usuário a partir do token." });
      }

      var success = await _donorService.UpdateDonorProfileAsync(donorId, dto);

      if (!success)
      {
        return NotFound(new { message = "Não foi possível encontrar ou atualizar o perfil do doador." });
      }
      return NoContent();
    }


        [Authorize]
        [HttpPut("me/password")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDTO dto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
      {
        return Unauthorized(new { message = "Não foi possível identificar o usuário a partir do token." });
      }

      var result = await _donorService.ChangePasswordAsync(donorId, dto);

      if (!result.Success)
      {
        return BadRequest(new { message = result.Message, errors = result.Errors });
      }
      return Ok(new { message = result.Message ?? "Senha alterada com sucesso." });
    }

        [Authorize]
        [HttpPost("me/picture")]
        public async Task<IActionResult> UpdateMyProfilePicture(IFormFile imageFile)
        {
          if (imageFile == null || imageFile.Length == 0)
          {
            return BadRequest(new { message = "Nenhum arquivo de imagem foi enviado." });
          }

          var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
          if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
          {
            return Unauthorized(new { message = "Não foi possível identificar o usuário a partir do token." });
          }

          try
          {
            var newImageUrl = await _donorService.UpdateProfilePictureAsync(donorId, imageFile);

            if (string.IsNullOrEmpty(newImageUrl))
            {
              return BadRequest(new { message = "Falha ao atualizar a foto do perfil. Verifique o arquivo ou tente novamente." });
            }

            return Ok(new { profilePictureUrl = newImageUrl });
          }
          catch (ArgumentException ex)
          {
            return BadRequest(new { message = ex.Message });
          }
        }

    [Authorize]
    [HttpDelete("me/picture")]
    public async Task<IActionResult> DeleteMyProfilePicture()
    {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
      {
        return Unauthorized(new { message = "Não foi possível identificar o usuário a partir do token." });
      }

      var success = await _donorService.DeleteProfilePictureAsync(donorId);

      if (!success)
      {
        return NotFound(new { message = "Não foi possível encontrar o doador ou remover a foto de perfil." });
      }
      return NoContent();
    }

    }

}
