namespace PetDoa.DTOs
{
  public class AdminDashboardStatsDto
  {
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int NewDonorsThisMonth { get; set; }
    public int DonationsThisMonth { get; set; }
  }
}
