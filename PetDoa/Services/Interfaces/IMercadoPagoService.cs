namespace PetDoa.Services.Interfaces
{
  public interface IMercadoPagoService
  {
    Task<string> CreatePaymentPreferenceAsync(decimal amount, string donationTitle, int ourDonationId);

  }
}
