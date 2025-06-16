namespace PetDoa.Services.PdfGenerator
{
  using PetDoa.Models;
  using QuestPDF.Fluent;
  using QuestPDF.Infrastructure;
  using QuestPDF.Helpers;
  using PetDoa.Models.Enums;

  public class ConsolidatedReceipt : IDocument
  {
    private readonly Donor _donor;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;
    private readonly List<Donation> _donations;

    private static readonly string PetDoaLogoSvg = @"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512'><path fill='#F4D35E' d='M256 288c-56 0-96 48-96 96 0 48 56 80 96 80s96-32 96-80c0-48-40-96-96-96zm-104-16c-31 0-56-29-56-64s25-64 56-64 56 29 56 64-25 64-56 64zm208 0c-31 0-56-29-56-64s25-64 56-64 56 29 56 64-25 64-56 64zm-160-80c0 35-25 64-56 64s-56-29-56-64 25-64 56-64 56 29 56 64zm192 0c0 35-25 64-56 64s-56-29-56-64 25-64 56-64 56 29 56 64z' /></svg>";

    public ConsolidatedReceipt(Donor donor, DateTime startDate, DateTime endDate, List<Donation> donations)
    {
      _donor = donor;
      _startDate = startDate;
      _endDate = endDate;
      _donations = donations;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
      container
        .Page(page =>
        {
          page.Margin(40);
          page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));


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
                  text.Span("Pet").SemiBold().FontSize(20).FontColor(Colors.Grey.Darken4);
                  text.Span("Doa").Bold().FontSize(20).FontColor(Colors.Teal.Accent4);
                });
              });
              col.Spacing(10);
              col.Item().Text("Relatório Consolidado de Doações").Bold().FontSize(14).FontColor(Colors.Grey.Darken2);
            });
            row.AutoItem().AlignRight().Column(col =>
            {
              col.Item().AlignRight().Text("Período de Referência");
              col.Item().AlignRight().Text($"{_startDate:dd/MM/yyyy} a {_endDate:dd/MM/yyyy}").SemiBold();
            });
          });

          page.Content().PaddingVertical(30).Column(col =>
          {
            col.Spacing(25);

            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
            {
              c.Item().Text("Resumo do Doador").SemiBold().FontSize(14).FontColor(Colors.Grey.Darken2);
              c.Spacing(5);
              c.Item().Text($"Doador(a): {_donor.Name}").Bold();
              c.Item().Text($"CPF: 123.456.789.10");
              c.Item().Text($"Total de Doações no Período: {_donations.Count}");
              c.Item().Text($"Valor Total Doado no Período: {_donations.Sum(d => d.Amount):C2}").SemiBold();
            });

            col.Item().Column(tabela =>
            {
              tabela.Item().Text("Detalhamento das Doações").Bold().FontSize(14).FontColor(Colors.Grey.Darken2);

              tabela.Item().Table(table =>
              {
                table.ColumnsDefinition(columns =>
                {
                  columns.RelativeColumn(2);
                  columns.RelativeColumn(3); 
                  columns.RelativeColumn(3);
                  columns.RelativeColumn(2);
                  columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                  header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Data").SemiBold();
                  header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Descrição").SemiBold();
                  header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Instituição").SemiBold();
                  header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Método").SemiBold();
                  header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Valor").SemiBold();
                });

                foreach (var donation in _donations)
                {
                 
                  table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{donation.Date:dd/MM/yyyy}");
                  var description = donation.ProductId.HasValue ? donation.ProductName : "Doação Avulsa";
                  table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(description);
                  table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(donation.ONG.Name);
                  table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(FormatPaymentMethod(donation.Method));
                  table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{donation.Amount:C2}");
                }
              });
            });
          });

          page.Footer().AlignCenter().Text(x =>
          {
            x.Span("Relatório gerado pela plataforma PetDoa em ");
            x.Span(DateTime.Now.ToString("dd/MM/yyyy"));
          });
        });
    }

    private string FormatPaymentMethod(PaymentMethod method)
    {
      return method switch
      {
        PaymentMethod.CreditCard => "Cartão de Crédito",
        PaymentMethod.Pix => "Pix",
        PaymentMethod.Boleto => "Boleto",
        PaymentMethod.Outro => "Saldo em Conta",
        _ => "Outro"
      };
    }
  }
}
