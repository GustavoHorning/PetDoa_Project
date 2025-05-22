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
        private readonly IConfiguration _configuration;

    public AuthController(IDonorService donorService, IAdminService adminService, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _donorService = donorService;
            _adminService = adminService;
            _logger = logger;
            _configuration = configuration;
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

          if (!result.Success || result.Data == null) 
          {
            return BadRequest(new { message = result.Message, errors = result.Errors });
          }

          var newDonor = result.Data;
          var appToken = await _donorService.GenerateJwtForDonorAsync(newDonor);

          if (appToken == null)
          {
            return Ok(new { message = result.Message + " (Erro ao gerar token de login automático)" });
          }

          return Ok(new
          {
            message = result.Message,
            token = appToken
          });
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
        public async Task<IActionResult> GoogleCallback(string? returnUrl = "/", string? remoteError = null)
        {
          if (remoteError != null)
          {
            // Usuário negou o acesso ou houve erro no Google
            // TODO: Redirecionar para uma página de erro/aviso no frontend
            string frontendErrorUrl = _configuration["FrontendAppUrl"] ?? "http://localhost:4200"; // URL base do frontend
            return Redirect($"{frontendErrorUrl}/login?error=google_auth_failed&message={Uri.EscapeDataString(remoteError)}");
          }

          var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
          if (info?.Succeeded != true || info.Principal == null)
          {
            string frontendErrorUrl = _configuration["FrontendAppUrl"] ?? "http://localhost:4200";
            return Redirect($"{frontendErrorUrl}/login?error=external_auth_error");
          }

          string? googleUserId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
          string? email = info.Principal.FindFirstValue(ClaimTypes.Email);
          string? firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
          string? lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
          string name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? $"{firstName} {lastName}".Trim();

          if (googleUserId == null || email == null)
          {
            string frontendErrorUrl = _configuration["FrontendAppUrl"] ?? "http://localhost:4200";
            return Redirect($"{frontendErrorUrl}/login?error=google_info_missing");
          }

          await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

          var result = await _donorService.FindOrCreateDonorByOAuthAsync("Google", googleUserId, email, name);

          // Obter a URL base do frontend da configuração
          // Adicione "FrontendAppUrl": "http://localhost:4200" ao seu appsettings.Development.json
          string frontendAppUrl = _configuration["FrontendAppUrl"] ?? "http://localhost:4200";
          string callbackReceiverPath = "/auth-callback"; // Rota no frontend que receberá o token

          if (result.Success && result.Donor != null)
          {
            var appToken = await _donorService.GenerateJwtForDonorAsync(result.Donor);
            if (appToken == null)
            {
              return Redirect($"{frontendAppUrl}{callbackReceiverPath}?error=token_generation_failed");
            }

            // Redireciona para o frontend passando o token como query parameter
            // Usar fragmento (#) é mais seguro para tokens, mas query param é mais simples de ler no Angular
            return Redirect($"{frontendAppUrl}{callbackReceiverPath}?token={Uri.EscapeDataString(appToken)}");
          }
          else if (result.RequiresPasswordLogin)
          {
            return Redirect($"{frontendAppUrl}{callbackReceiverPath}?error=email_conflict&message={Uri.EscapeDataString(result.ErrorMessage ?? "")}");
          }
          else
          {
            return Redirect($"{frontendAppUrl}{callbackReceiverPath}?error=social_processing_error&message={Uri.EscapeDataString(result.ErrorMessage ?? "")}");
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
