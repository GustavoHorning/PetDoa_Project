using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PetDoa.DTOs;
using PetDoa.Services.Interfaces;

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IDonorService _donorService;
        private readonly IAdminService _adminService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IDonorService donorService, IAdminService adminService, ILogger<AuthController> logger)
        {
            _donorService = donorService;
            _adminService = adminService;
            _logger = logger;
        }


        [AllowAnonymous]
        [HttpPost("donor/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDonorDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _donorService.RegisterDonorAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }
            return Ok(new { message = result.Message });
        }

        [AllowAnonymous]
        [HttpPost("donor/login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginDonorDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _donorService.LoginDonorAsync(dto);

            if (token == null)
            {
                return Unauthorized(new { message = "E-mail ou senha inválidos." });
            }
            return Ok(new { Token = token });
        }

        [AllowAnonymous]
        [HttpGet("google/login")]
        public IActionResult GoogleLoginTest(string? returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = "/")
        {
            var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (info?.Succeeded != true || info.Principal == null)
            {
                return BadRequest(new { message = "Erro durante a autenticação externa." });
            }

            string? googleUserId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            string? email = info.Principal.FindFirstValue(ClaimTypes.Email);
            string? firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            string? lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            string name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? $"{firstName} {lastName}".Trim();


            if (googleUserId == null || email == null)
            {
                return BadRequest(new { message = "Informações essenciais (ID ou Email) não recebidas do Google." });
            }

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var result = await _donorService.FindOrCreateDonorByOAuthAsync("Google", googleUserId, email, name);

            if (result.Success && result.Donor != null)
            {
                var appToken = await _donorService.GenerateJwtForDonorAsync(result.Donor);

                if (appToken == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Não foi possível gerar o token de acesso interno." });
                }
                return Ok(new { Token = appToken });
            }
            else if (result.RequiresPasswordLogin)
            {
                return Conflict(new { message = result.ErrorMessage });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = result.ErrorMessage ?? "Ocorreu um erro ao processar o login social." });
            }
        }

        [AllowAnonymous]
        [HttpPost("admin/login")]
        public async Task<ActionResult<AdminLoginResponseDTO>> LoginAdministratorAsync([FromBody] AdminLoginDTO dto)
        {
            var response = await _adminService.AuthenticateAsync(dto);

            if (response == null)
            {
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }

            return Ok(response);
        }
    }
}
