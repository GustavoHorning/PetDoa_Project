// PetDoa/Services/AdminService.cs

// Adicione os usings necessários
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

namespace PetDoa.Services
{
    // Faça a classe implementar a interface IAdminService
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<Administrator> _passwordHasher;
        private readonly IMapper _mapper; // Injete o IMapper

        // Atualize o construtor para incluir IMapper
        public AdminService(AppDbContext context, IConfiguration config, IPasswordHasher<Administrator> passwordHasher, IMapper mapper)
        {
            _context = context;
            _config = config;
            _passwordHasher = passwordHasher;
            _mapper = mapper; // Armazene o mapper
        }

        // --- Método AuthenticateAsync (já existente) ---
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

        // --- Implementação dos novos métodos CRUD ---

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
            // 1. Verificar se a ONG existe
            var ongExists = await _context.ONGs.AnyAsync(o => o.ID == createDto.OngId);
            if (!ongExists)
            {
                // Lançar exceção ou retornar um resultado indicando erro.
                // Lançar exceção é comum em serviços quando um pré-requisito falha.
                throw new ArgumentException($"ONG com ID {createDto.OngId} não encontrada.");
                // Alternativa: retornar um ServiceResponse com erro.
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


        // --- Método GenerateJwtToken (privado, já existente) ---
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
                expires: DateTime.UtcNow.AddHours(2), // ou leia do config
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}