using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetDoa.Data;
using PetDoa.DTOs;
using PetDoa.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PetDoa.Services.Interfaces;
using PetDoa.Models.Enums;

namespace PetDoa.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<Administrator> _passwordHasher;
        private readonly IMapper _mapper;

        public AdminService(AppDbContext context, IConfiguration config, IPasswordHasher<Administrator> passwordHasher, IMapper mapper)
        {
            _context = context;
            _config = config;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<AdminLoginResponseDTO?> AuthenticateAsync(AdminLoginDTO dto)
        {
            var admin = await _context.Administrators
                .FirstOrDefaultAsync(a => a.Email == dto.Email);

            if (admin == null || _passwordHasher.VerifyHashedPassword(admin, admin.Password_Hash, dto.Password) == PasswordVerificationResult.Failed)
                return null;

            var token = GenerateJwtToken(admin);

            return new AdminLoginResponseDTO
            {
                Token = token,
                Role = admin.IsSuperAdmin ? "SuperAdmin" : "Admin"
            };
        }

        public async Task<IEnumerable<AdminReadDTO>> GetAdministratorsAsync()
        {
            var admins = await _context.Administrators.ToListAsync();
            return _mapper.Map<List<AdminReadDTO>>(admins);
        }

        public async Task<AdminReadDTO?> GetAdminByIdAsync(int id)
        {
            var admin = await _context.Administrators
                                   .FirstOrDefaultAsync(a => a.ID == id);
            return _mapper.Map<AdminReadDTO?>(admin);
        }

        public async Task<AdminReadDTO> CreateAdminAsync(CreateAdminApiDTO createDto)
        {
            var ongExists = await _context.ONGs.AnyAsync(o => o.ID == createDto.OngId);
            if (!ongExists)
            {
                throw new ArgumentException($"ONG com ID {createDto.OngId} não encontrada.");
            }

            // 1.1 Verificar se o email já existe (opcional mas recomendado)
            var emailExists = await _context.Administrators.AnyAsync(a => a.Email == createDto.Email);
            if (emailExists)
            {
                throw new ArgumentException($"O e-mail '{createDto.Email}' já está em uso.");
            }

            // 2. Criar a entidade Administrator
            var admin = new Administrator
            {
                Name = createDto.Name,
                Email = createDto.Email,
                OngId = createDto.OngId,
                Registration_Date = DateTime.UtcNow, // Definir data/hora aqui
                // IsSuperAdmin = createDto.IsSuperAdmin // Se você adicionou ao DTO
            };

            // 3. Hashear a senha vinda do DTO (texto plano)
            admin.Password_Hash = _passwordHasher.HashPassword(admin, createDto.Password);

            // 4. Adicionar e Salvar
            _context.Administrators.Add(admin);
            await _context.SaveChangesAsync();

            // 5. Mapear para DTO de leitura e retornar
            return _mapper.Map<AdminReadDTO>(admin);
        }

        public async Task<bool> DeleteAdminAsync(int id)
        {
            var admin = await _context.Administrators.FindAsync(id);
            if (admin == null)
            {
                return false;
            }

            _context.Administrators.Remove(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateJwtToken(Administrator admin)
        {
            var role = admin.IsSuperAdmin ? "SuperAdmin" : "Admin";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.ID.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim("name", admin.Name),
                new Claim(ClaimTypes.Role, role)
            };

            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    public async Task<AdminDashboardDto> GetAdminDashboardDataAsync()
    {
      var today = DateTime.UtcNow.Date;
      var startOfMonth = new DateTime(today.Year, today.Month, 1);
      var startOf30Days = today.AddDays(-29);

      var stats = new AdminDashboardStatsDto
      {
        RevenueToday = await _context.Donations
              .Where(d => d.Status == DonationStatus.Completed && d.Date >= today)
              .SumAsync(d => (decimal?)d.Amount) ?? 0,

        RevenueThisMonth = await _context.Donations
              .Where(d => d.Status == DonationStatus.Completed && d.Date >= startOfMonth)
              .SumAsync(d => (decimal?)d.Amount) ?? 0,

        NewDonorsThisMonth = await _context.Donors
              .CountAsync(d => d.Registration_Date >= startOfMonth),

        DonationsThisMonth = await _context.Donations
              .CountAsync(d => d.Status == DonationStatus.Completed && d.Date >= startOfMonth)
      };

      var recentDonations = await _context.Donations
        .Where(d => d.Status == DonationStatus.Completed && d.Date >= startOf30Days)
        .Select(d => new { d.Date, d.Amount }) // Pegamos apenas os campos necessários
        .ToListAsync();

      // Agora, agrupamos e somamos em memória, o que é mais seguro
      var dailyRevenue = recentDonations
          .GroupBy(d => d.Date.Date)
          .Select(g => new DailyRevenueDto
          {
            Date = g.Key.ToString("dd/MM"),
            Amount = g.Sum(d => d.Amount)
          })
          .OrderBy(r => r.Date)
          .ToList();

      var dashboardData = new AdminDashboardDto
      {
        Stats = stats,
        DailyRevenueLast30Days = dailyRevenue
      };

      return dashboardData;
    }


    public async Task<PaginatedResultDto<AdminDonorListDto>> GetAllDonorsAsync(int pageNumber, int pageSize, string? searchTerm)
    {
      // 1. Começamos com a consulta base de todos os doadores
      var query = _context.Donors.AsQueryable();

      // 2. Se um termo de busca foi fornecido, filtramos por nome ou e-mail
      if (!string.IsNullOrEmpty(searchTerm))
      {
        query = query.Where(d => d.Name.Contains(searchTerm) || d.Email.Contains(searchTerm));
      }

      // 3. Contamos o total de resultados ANTES de paginar
      var totalCount = await query.CountAsync();

      // 4. Executamos a consulta, aplicando ordenação e paginação
      var donors = await query
          .OrderByDescending(d => d.Registration_Date)
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .Select(d => new AdminDonorListDto
          {
            // Mapeamos os dados manualmente para o DTO, incluindo os cálculos
            Id = d.ID,
            Name = d.Name,
            Email = d.Email,
            RegistrationDate = d.Registration_Date,
            // Contamos apenas as doações completadas
            DonationCount = d.Donations.Count(don => don.Status == DonationStatus.Completed),
            // Somamos apenas o valor das doações completadas
            TotalDonated = d.Donations.Where(don => don.Status == DonationStatus.Completed).Sum(don => (decimal?)don.Amount) ?? 0
          })
          .ToListAsync();

      // 5. Retornamos o resultado paginado
      return new PaginatedResultDto<AdminDonorListDto>(donors, totalCount, pageNumber, pageSize);
    }

    public async Task<AdminDonorDetailDto?> GetDonorDetailsAsync(int donorId)
    {
      var donor = await _context.Donors
          .Include(d => d.Donations) // Incluímos o histórico de doações
              .ThenInclude(don => don.ONG) // E a ONG de cada doação
          .Where(d => d.ID == donorId)
          .Select(d => new AdminDonorDetailDto
          {
            Id = d.ID,
            Name = d.Name,
            Email = d.Email,
            RegistrationDate = d.Registration_Date,
            DonationCount = d.Donations.Count(don => don.Status == DonationStatus.Completed),
            TotalDonated = d.Donations.Where(don => don.Status == DonationStatus.Completed).Sum(don => (decimal?)don.Amount) ?? 0,
            DonationHistory = _mapper.Map<List<DonationReadDTO>>(d.Donations.OrderByDescending(don => don.Date).ToList())
          })
          .FirstOrDefaultAsync();

      return donor;
    }


  }
}
