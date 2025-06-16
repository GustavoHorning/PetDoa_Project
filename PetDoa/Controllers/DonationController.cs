using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetDoa.DTOs;
using PetDoa.Models.Enums;
using PetDoa.Services;
using PetDoa.Services.Interfaces;
using CsvHelper;
using PetDoa.Data;


namespace PetDoa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;
        private readonly AppDbContext _context;
        private readonly ILogger<DonationController> _logger;


    public DonationController(IDonationService donationService, AppDbContext context, ILogger<DonationController> loger)
        {
            _donationService = donationService;
            _context = context;
            _logger = loger;
        }

        public class DashboardSummaryDto
        {
          public decimal TotalDonated { get; set; }
          public DateTime? LastDonationDate { get; set; }
          public string DonorName { get; set; }
          public int ItemsDonatedCount { get; set; }
        }

        [Authorize] 
        [HttpPost]
        public async Task<ActionResult<DonationReadDTO>> CreateDonation(CreateDonationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
            {
                return Unauthorized("Não foi possível identificar o usuário.");
            }

            try
            {
                var createdDonationDto = await _donationService.CreateDonationAsync(dto, donorId);

                return CreatedAtAction(nameof(GetDonation), new { id = createdDonationDto.Id }, createdDonationDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetAllDonations()
        {
            var donations = await _donationService.GetAllDonationsAsync();
            return Ok(donations);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DonationReadDTO>> GetDonation(int id)
        {
            var donationDto = await _donationService.GetDonationByIdAsync(id);

            if (donationDto == null)
            {
                return NotFound();
            }

            return Ok(donationDto);
        }

    //[Authorize]
    //[HttpGet("me")]
    //public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetMyDonations()
    //{
    //  var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
    //    {
    //        return Unauthorized("Não foi possível identificar o usuário.");
    //    }

    //    var donations = await _donationService.GetDonationsByDonorAsync(donorId);

    //    return Ok(donations);
    //}

    // Em PetDoa/Controllers/DonationController.cs

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<PaginatedResultDto<DonationReadDTO>>> GetMyDonations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
      {
        return Unauthorized("Não foi possível identificar o usuário.");
      }

      var paginatedDonations = await _donationService.GetDonationsByDonorAsync(donorId, pageNumber, pageSize);

      return Ok(paginatedDonations);
    }


        [Authorize(Roles = "Admin,SuperAdmin")]
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


        [Authorize]
        [HttpGet("my-summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetMyDonationSummary()
        {
          var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
          if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
          {
            return Unauthorized("Não foi possível identificar o usuário.");
          }

          var summary = await _donationService.GetDonationSummaryByDonorAsync(donorId);

          if (summary == null)
          {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            return Ok(new DashboardSummaryDto { TotalDonated = 0, DonorName = userName });
          }

          return Ok(summary);
        }


    [Authorize]
    [HttpGet("{id}/receipt")]
    public async Task<IActionResult> DownloadReceipt(int id)
    {
      try
      {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int donorId))
        {
          return Unauthorized("Usuário inválido.");
        }

        var pdfBytes = await _donationService.GenerateReceiptPdfAsync(id, donorId);

        return File(pdfBytes, "application/pdf", $"recibo-doacao-{id}.pdf");
      }
      catch (UnauthorizedAccessException ex)
      {

        return Forbid(ex.Message);
      }
      catch (Exception ex)
      {

        return StatusCode(500, "Ocorreu um erro ao gerar o recibo.");
      }
    }



    [Authorize]
    [HttpGet("report")]
    public async Task<IActionResult> DownloadConsolidatedReport(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate)
    {

      if (endDate > DateTime.UtcNow)
      {
        return BadRequest(new { message = "A data final do relatório não pode ser no futuro." });
      }

      if (startDate > endDate)
      {
        return BadRequest(new { message = "A data de início não pode ser posterior à data final." });
      }

      try
      {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out int donorId))
        {
          return Unauthorized("Usuário inválido.");
        }

        var pdfBytes = await _donationService.GenerateConsolidatedReportPdfAsync(donorId, startDate, endDate);

        var fileName = $"relatorio-doacoes-{startDate:yyyy-MM-dd}-a-{endDate:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
      }
      catch (Exception ex)
      {
        return StatusCode(500, "Ocorreu um erro ao gerar o relatório.");
      }
    }

    [Authorize]
    [HttpGet("me/recent")]
    public async Task<ActionResult<IEnumerable<DonationReadDTO>>> GetMyRecentDonations([FromQuery] int limit = 3)
    {
      var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int donorId))
      {
        return Unauthorized("Não foi possível identificar o usuário.");
      }

      var recentDonations = await _donationService.GetRecentDonationsByDonorAsync(donorId, limit);
      return Ok(recentDonations);
    }


    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("export")] // Rota: GET /api/donations/export?startDate=...&endDate=...
    public async Task<IActionResult> ExportDonationsToCsv(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate)
    {
      try
      {
        // 1. Buscamos todas as doações no período, incluindo os dados relacionados
        var inclusiveEndDate = endDate.AddDays(1);
        var donations = await _context.Donations
            .Include(d => d.Donor)
            .Where(d => d.Status == DonationStatus.Completed &&
                         d.Date >= startDate &&
                         d.Date < inclusiveEndDate)
            .OrderBy(d => d.Date)
            .Select(d => new
            {
              Data = d.Date.ToString("dd/MM/yyyy HH:mm"),
              Doador = d.Donor.Name,
              Email = d.Donor.Email,
              Tipo = d.ProductId.HasValue ? "Produto" : "Doação Avulsa",
              Descricao = d.ProductId.HasValue ? d.ProductName : "Doação Monetária",
              Metodo = d.Method.ToString(),
              Valor = d.Amount
            })
            .ToListAsync();

        using (var memoryStream = new MemoryStream())
        using (var streamWriter = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
          await csvWriter.WriteRecordsAsync(donations);
          await streamWriter.FlushAsync(); // Garante que tudo foi escrito no stream

          var fileName = $"relatorio-doacoes-{startDate:yyyy-MM-dd}-a-{endDate:yyyy-MM-dd}.csv";
          return File(memoryStream.ToArray(), "text/csv", fileName);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro ao exportar doações para CSV.");
        return StatusCode(500, "Ocorreu um erro ao gerar o relatório CSV.");
      }
    }


  }
}
