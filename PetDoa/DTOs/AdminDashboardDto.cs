namespace PetDoa.DTOs
{
  public class AdminDashboardDto
  {
    public AdminDashboardStatsDto Stats { get; set; }
    public List<DailyRevenueDto> DailyRevenueLast30Days { get; set; }
  }
}
