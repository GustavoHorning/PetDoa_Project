namespace PetDoa.Services.PdfGenerator
{

  using PetDoa.Models;
  using PetDoa.Models.Enums;
  using QuestPDF.Fluent;
  using QuestPDF.Helpers;
  using QuestPDF.Infrastructure;

  public class DonationReceipt : IDocument
  {
    private readonly Donation _donation;
    private static readonly string PetDoaLogoSvg = @"
            <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512'>
              <path fill='#F4D35E' d='M256 288c-56 0-96 48-96 96 0 48 56 80 96 80s96-32 96-80c0-48-40-96-96-96zm-104-16c-31 0-56-29-56-64s25-64 56-64 56 29 56 64-25 64-56 64zm208 0c-31 0-56-29-56-64s25-64 56-64 56 29 56 64-25 64-56 64zm-160-80c0 35-25 64-56 64s-56-29-56-64 25-64 56-64 56 29 56 64zm192 0c0 35-25 64-56 64s-56-29-56-64 25-64 56-64 56 29 56 64z' />
            </svg>";



    public DonationReceipt(Donation donation)
    {
      _donation = donation;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
      container
          .Page(page =>
          {
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));
            var title = _donation.ProductId.HasValue ? "Recibo de Compra Solidária" : "Recibo de Doação";
            var localDonationDate = _donation.Date.ToLocalTime();


            page.Header().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(10).Row(row =>
            {
              row.RelativeItem().Column(col =>
              {
                col.Item().Row(logoRow =>
                {
                  logoRow.ConstantItem(32).AlignMiddle().Svg(PetDoaLogoSvg);
                  logoRow.Spacing(8);
                  logoRow.RelativeItem().AlignMiddle().Text(text =>
                  {
                    text.Span("Pet").SemiBold().FontSize(25).FontColor(Colors.Grey.Darken4);
                    text.Span("Doa").Bold().FontSize(25).FontColor(Colors.Teal.Accent4);
                  });
                });
                col.Spacing(10);
                col.Item().Text(title).Bold().FontSize(14).FontColor(Colors.Grey.Darken2);
              });
              row.AutoItem().AlignRight().Column(col =>
              {
                col.Item().AlignRight().Text($"Recibo #{_donation.ID}");
                col.Item().AlignRight().Text($"Emitido em: {localDonationDate:dd/MM/yyyy HH:mm}");
              });
            });

            page.Content().PaddingVertical(30).Column(col =>
            {
              col.Spacing(30);

              col.Item().Row(row =>
              {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                {
                  c.Spacing(5);
                  c.Item().Text("Recebido por").SemiBold().FontColor(Colors.Grey.Darken2);
                  c.Item().Text(_donation.ONG.Name).Bold();
                  c.Item().Text($"CNPJ: {_donation.ONG.Cnpj}");
                  c.Item().Text($"{_donation.ONG.Street}, {_donation.ONG.Number}");
                  c.Item().Text($"{_donation.ONG.City} - {_donation.ONG.State}, CEP: {_donation.ONG.ZipCode}");
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                {
                  c.Spacing(5);
                  c.Item().Text("Doador(a)").SemiBold().FontColor(Colors.Grey.Darken2);
                  c.Item().Text(_donation.Donor?.Name ?? "Nome do Doador Fictício").Bold();
                  c.Item().Text("CPF: 123.456.789-00");
                });
              });

              col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Column(table =>
              {
                table.Item().Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
                {
                  row.RelativeItem().Text("Descrição").Bold();
                  row.ConstantItem(100).AlignRight().Text("Valor").Bold();
                });

                table.Item().Padding(8).Row(row =>
                {
                  row.RelativeItem().Column(c =>
                  {
                    var description = _donation.ProductId.HasValue
                               ? $"Compra do item: {_donation.ProductName}"
                               : $"Doação para: {_donation.ONG.Cause}";

                    c.Item().Text(description).SemiBold();
                    c.Item().Text($"Pagamento via {FormatPaymentMethod(_donation.Method)}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    c.Item().Text($"ID da Transação: {_donation.GatewayPaymentId}").FontSize(9).FontColor(Colors.Grey.Darken1);
                  });
                  row.ConstantItem(100).AlignRight().Text($"{_donation.Amount:C2}").SemiBold();
                });

                table.Item().Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
                {
                  row.RelativeItem().AlignRight().Text("Total Pago").Bold();
                  row.ConstantItem(100).AlignRight().Text($"{_donation.Amount:C2}").Bold();
                });
              });

              col.Item().AlignCenter().Text(
                $"Obrigado, {_donation.Donor?.Name ?? "Doador(a)"}! Sua doação foi aprovada em {localDonationDate:dd/MM/yyyy} e fará uma grande diferença. ❤️")
                    .FontSize(10).Italic();
            });

            page.Footer().AlignCenter().Text(x =>
            {
              x.Span("Recibo gerado pela plataforma PetDoa em ");
              x.Span(DateTime.Now.ToString("dd/MM/yyyy"));
            });
          });
    }

    private string FormatPaymentMethod(PaymentMethod method)
    {
      switch (method)
      {
        case PaymentMethod.CreditCard: return "Cartão de Crédito";
        case PaymentMethod.Pix: return "Pix";
        case PaymentMethod.Boleto: return "Boleto";
        case PaymentMethod.Outro: return "Saldo em Conta";
        default: return "Outro";
      }
    }
  }
}
