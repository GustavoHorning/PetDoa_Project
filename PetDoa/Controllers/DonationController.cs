
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetDoa.DTOs; // Para DonationReadDTO, CreateDonationDTO
using PetDoa.Services.Interfaces; // Adicione para IDonationService

namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        // Atualize o construtor:
        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        // POST api/donation - Cria uma nova doação
        // AGORA REQUER AUTENTICAÇÃO
        [Authorize] // Garante que apenas usuários logados podem doar
        [HttpPost]
        public async Task<ActionResult<DonationReadDTO>> CreateDonation(CreateDonationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obter o ID do doador a partir do token JWT (Claims)
            // Usamos NameIdentifier que é padrão para ID do usuário, ou a claim "DonorID" que você criou.
            // É mais seguro usar NameIdentifier se ele contiver o ID do Donor. Vamos assumir NameIdentifier.
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
            {
                // Isso não deveria acontecer se [Authorize] estiver funcionando, mas é uma segurança extra.
                return Unauthorized("Não foi possível identificar o usuário.");
            }

            try
            {
                // Chama o serviço passando o DTO e o ID do doador autenticado
                var createdDonationDto = await _donationService.CreateDonationAsync(dto, donorId);

                // Retorna CreatedAtAction com o DTO retornado pelo serviço
                return CreatedAtAction(nameof(GetDonation), new { id = createdDonationDto.Id }, createdDonationDto);
            }
            catch (ArgumentException ex) // Captura erros de validação do serviço (ONG/Doador não existe, valor inválido)
            {
                return BadRequest(new { message = ex.Message });
            }
            // Capturar outras exceções gerais se necessário
        }

        // GET api/donation - Lista TODAS as doações
        // PROTEGER ESTE ENDPOINT - Geralmente apenas para Admins
        [Authorize(Roles = "Admin,SuperAdmin")] // Exemplo de proteção
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetAllDonations()
        {
            var donations = await _donationService.GetAllDonationsAsync();
            return Ok(donations);
        }

        // GET api/donation/{id} - Busca uma doação específica
        // PROTEGER ESTE ENDPOINT - Geralmente apenas para Admins ou o próprio Doador (regra mais complexa)
        [Authorize(Roles = "Admin,SuperAdmin")] // Exemplo de proteção simples
        [HttpGet("{id}")]
        public async Task<ActionResult<DonationReadDTO>> GetDonation(int id)
        {
            var donationDto = await _donationService.GetDonationByIdAsync(id);

            if (donationDto == null)
            {
                return NotFound();
            }

            // Aqui poderia haver uma lógica extra para verificar se o usuário logado
            // é o dono da doação, caso não seja admin. Mas para simplificar, deixamos só para admin.

            return Ok(donationDto);
        }

        // GET api/donation/me - Busca as doações do usuário logado
        // Protegido por Authorize (qualquer usuário logado)
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetMyDonations()
        {
            // Obter o ID do doador a partir do token JWT (Claims)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // ou "DonorID"
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
            {
                return Unauthorized("Não foi possível identificar o usuário.");
            }

            // Chama o serviço para buscar as doações do doador específico
            var donations = await _donationService.GetDonationsByDonorAsync(donorId);

            // Retorna Ok com a lista de doações (pode ser vazia)
            return Ok(donations);
        }


        // DELETE api/donation/{id} - Deleta uma doação
        // PROTEGER ESTE ENDPOINT - Geralmente apenas para Admins
        [Authorize(Roles = "Admin,SuperAdmin")] // Exemplo de proteção
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonation(int id)
        {
            var deleted = await _donationService.DeleteDonationAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}