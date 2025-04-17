using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetDoa.Data;
using PetDoa.Dtos.Donor;
using PetDoa.DTOs;
using PetDoa.Models;
using PetDoa.Services.Interfaces;
using PetDoa.Util;

namespace PetDoa.Services
{
    public class DonorService : IDonorService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Donor> _passwordHasher;
        private readonly IConfiguration _configuration;


        public DonorService(AppDbContext context, IPasswordHasher<Donor> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }


        public async Task<ServiceResponse<Donor>> RegisterDonorAsync(RegisterDonorDto dto)
        {
            var response = new ServiceResponse<Donor>();

            // Verifica se as senhas coincidem
            if (dto.Password != dto.ConfirmPassword)
            {
                response.Success = false;
                response.Message = "As senhas não coincidem.";
                return response;
            }

            // Valida a senha e acumula os erros
            var passwordErrors = new List<string>();
            if (!PasswordValidator.IsValid(dto.Password, out passwordErrors))
            {
                response.Success = false;
                response.Message = "A senha não atende aos requisitos:";
                response.Errors = passwordErrors;
                return response;
            }

            // Verifica se o e-mail já está em uso
            var existing = await _context.Donors.FirstOrDefaultAsync(d => d.Email == dto.Email);
            if (existing != null)
            {
                response.Success = false;
                response.Message = "Já existe um doador com este e-mail.";
                return response;
            }

            // Cria o novo doador
            var donor = new Donor
            {
                Name = dto.FullName,
                Email = dto.Email,
                Registration_Date = DateTime.UtcNow
            };

            // Criptografa a senha
            donor.Password_Hash = _passwordHasher.HashPassword(donor, dto.Password);

            // Salva o novo doador no banco de dados
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            // Sucesso
            response.Data = donor;
            response.Message = "Doador registrado com sucesso!";
            response.Success = true;

            return response;
        }

        public async Task<string?> LoginDonorAsync(LoginDonorDto dto)
        {
            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Email == dto.Email);

            if (donor == null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(donor, donor.Password_Hash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            // Geração do token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, donor.ID.ToString()),
                    new Claim(ClaimTypes.Email, donor.Email),
                    new Claim(ClaimTypes.Name, donor.Name)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
    }
}
