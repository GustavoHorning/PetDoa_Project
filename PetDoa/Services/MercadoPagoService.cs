using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using PetDoa.Services.Interfaces;
using System.Text.Json;

namespace PetDoa.Services
{
  public class MercadoPagoService : IMercadoPagoService
  {
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<MercadoPagoService> _logger; // <-- Adicionado


    public MercadoPagoService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<MercadoPagoService> logger)
    {
      _configuration = configuration;
      _httpContextAccessor = httpContextAccessor;
      _logger = logger;
    }

    public async Task<string> CreatePaymentPreferenceAsync(decimal amount, string donationTitle, int ourDonationId)
    {
      _logger.LogInformation("Iniciando criação de preferência de pagamento para Doação ID: {DonationId}", ourDonationId);

      MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];

      //var request = _httpContextAccessor.HttpContext.Request;
      var baseUrl = await GetNgrokPublicUrlAsync();
      var notificationUrl = $"{baseUrl}/api/payments/webhook";

      var successUrl = $"{baseUrl}/donations/success";

      var requestOptions = new PreferenceRequest
      {
        Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = donationTitle,
                        Quantity = 1,
                        CurrencyId = "BRL",
                        UnitPrice = amount,
                    },
                },
        BackUrls = new PreferenceBackUrlsRequest
        {
          Success = successUrl,
        },
        NotificationUrl = notificationUrl,
        ExternalReference = ourDonationId.ToString(),
      };
      try
      {
        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(requestOptions);

        _logger.LogInformation("Preferência de pagamento criada com sucesso. InitPoint: {InitPoint}", preference.InitPoint);

        return preference.InitPoint;
      }
      catch (Exception ex)
      {
        // Este log vai capturar o erro exato se a criação da preferência falhar.
        _logger.LogError(ex, "Erro ao criar preferência de pagamento no Mercado Pago.");
        // Lançamos a exceção para que o controller saiba que algo deu errado.
        throw;
      }
    }


    private async Task<string> GetNgrokPublicUrlAsync()
    {
      using var httpClient = new HttpClient();
      var response = await httpClient.GetStringAsync("http://127.0.0.1:4040/api/tunnels");

      var json = JsonDocument.Parse(response);
      var tunnels = json.RootElement.GetProperty("tunnels");

      foreach (var tunnel in tunnels.EnumerateArray())
      {
        if (tunnel.GetProperty("proto").GetString() == "https")
        {
          return tunnel.GetProperty("public_url").GetString();
        }
      }

      throw new Exception("ngrok public URL not found");
    }

  }
}
