using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetDoa.Data;
using PetDoa.Models;
using PetDoa.Models.Enums;
using PetDoa.Services.Interfaces;
using System.Security.Claims;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Microsoft.EntityFrameworkCore;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using PetDoa.Models.Enums;
using PaymentMethod = PetDoa.Models.Enums.PaymentMethod;

namespace PetDoa.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class PaymentsController : ControllerBase
  {
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentsController> _logger; // <-- Adicionado
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor; // <-- 1. ADICIONE ESTA LINHA



    public PaymentsController(IMercadoPagoService mercadoPagoService, AppDbContext context, ILogger<PaymentsController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
      _mercadoPagoService = mercadoPagoService;
      _context = context;
      _logger = logger;
      _configuration = configuration;
      _httpContextAccessor = httpContextAccessor;
    }

    public class MercadoPagoNotification
    {
      public string action { get; set; }
      public DataPayload data { get; set; }
    }

    public class DataPayload
    {
      public string id { get; set; }
    }

    public class CreatePaymentRequest
    {
      public decimal Amount { get; set; }
    }

    [HttpPost("checkout")]
    [Authorize] 
    public async Task<IActionResult> CreatePaymentPreference([FromBody] CreatePaymentRequest request)
    {
      try
      {
        var donorIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(donorIdString) || !int.TryParse(donorIdString, out int donorId))
        {
          return Unauthorized("Não foi possível identificar o usuário a partir do token.");
        }

        var donation = new Donation
        {
          Amount = request.Amount,
          Date = DateTime.UtcNow,
          Status = DonationStatus.Pending,
          DonorID = donorId,
          OngId = 1 
        };
        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        var donationTitle = $"Doação para ONG Coração Animal - ID {donation.ID}";

        var paymentUrl = await _mercadoPagoService.CreatePaymentPreferenceAsync(
            request.Amount,
            donationTitle,
            donation.ID 
        );

        return Ok(new { PaymentUrl = paymentUrl });
      }
      catch (Exception ex)
      {
        return StatusCode(500, "Ocorreu um erro interno ao processar o pagamento.");
      }
    }


    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> MercadoPagoWebhook([FromBody] MercadoPagoNotification notification)
    {
      // Log para sabermos que o webhook foi chamado
      _logger.LogInformation("Webhook do Mercado Pago recebido. Ação: {Action}, Data ID: {DataId}",
          notification?.action, notification?.data?.id);

      try
      {

        if (notification != null && !string.IsNullOrEmpty(notification.action) && notification.action.StartsWith("payment."))
        {
          var paymentId = long.Parse(notification.data.id);

          var client = new PaymentClient();
          Payment payment = await client.GetAsync(paymentId);

          // Log para vermos os detalhes do pagamento que buscamos
          _logger.LogInformation("Pagamento ID {PaymentId} buscado. Status: {Status}, Ref Externa: {ExternalRef}",
              payment?.Id, payment?.Status, payment?.ExternalReference);

          if (payment != null && payment.Status == "approved")
          {
            var externalReference = payment.ExternalReference;
            if (int.TryParse(externalReference, out int donationId))
            {
              var donation = await _context.Donations.FirstOrDefaultAsync(d => d.ID == donationId);

              if (donation != null && donation.Status == DonationStatus.Pending)
              {
                // --- INÍCIO DAS ALTERAÇÕES ---

                // 1. Atualiza o status para Completo
                donation.Status = DonationStatus.Completed;

                // 2. Salva o ID do pagamento do Mercado Pago (ex: 114027114899)
                donation.GatewayPaymentId = payment.Id.ToString();

                // 3. "Traduz" o método de pagamento de texto para o seu Enum numérico
                PaymentMethod paymentMethodEnum;
                switch (payment.PaymentTypeId?.ToLower())
                {
                  case "credit_card":
                    paymentMethodEnum = PaymentMethod.CreditCard;
                    break;
                  case "pix":
                    paymentMethodEnum = PaymentMethod.Pix;
                    break;
                  case "ticket": // "ticket" é como o Mercado Pago chama o Boleto
                    paymentMethodEnum = PaymentMethod.Boleto;
                    break;
                  default:
                    // Um valor padrão caso o método seja desconhecido
                    paymentMethodEnum = PaymentMethod.Outro;
                    break;
                }
                // Atribui o valor do Enum (que será salvo como número no banco)
                donation.Method = paymentMethodEnum;

                // --- FIM DAS ALTERAÇÕES ---

                _context.Donations.Update(donation);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Doação ID {DonationId} atualizada com sucesso. Status: {Status}, GatewayID: {GatewayId}, Método: {Method}",
                    donation.ID,
                    donation.Status,
                    donation.GatewayPaymentId,
                    donation.Method);
              }
              else
              {
                _logger.LogWarning("Doação ID {DonationId} não encontrada no banco ou já processada.", donationId);
              }
            }
          }
        }
        return Ok();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro crítico ao processar webhook do Mercado Pago.");
        // Mesmo com erro, retornamos OK para evitar que o Mercado Pago envie notificações repetidas
        return Ok();
      }
    }


  }


}
