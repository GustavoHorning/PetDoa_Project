using Microsoft.AspNetCore.Identity;
using PetDoa.Data;
using PetDoa.Models;
using System;
using System.Linq;

using Microsoft.AspNetCore.Identity;
using PetDoa.Models;
using System;
using System.Linq;

namespace PetDoa.Models
{
    public class AdminSeeder
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Administrator> _passwordHasher;

        public AdminSeeder(AppDbContext context, IPasswordHasher<Administrator> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public void SeedSuperAdmins()
        {
            // Verifica se já existem Super Admins
            if (!_context.Administrators.Any())
            {
                // Criação de Super Admins
                var superAdmin1 = new Administrator
                {
                    Name = "Super Admin 1",
                    Email = "superadmin1@petdoa.com",
                    Registration_Date = DateTime.Now,
                    OngId = 1, // Se for a única ONG
                    IsSuperAdmin = true
                };

                var superAdmin2 = new Administrator
                {
                    Name = "Super Admin 2",
                    Email = "superadmin2@petdoa.com",
                    Registration_Date = DateTime.Now,
                    OngId = 1, // Se for a única ONG
                    IsSuperAdmin = true
                };

                // Hashing das senhas
                superAdmin1.Password_Hash = _passwordHasher.HashPassword(superAdmin1, "AdminMaster@1");
                superAdmin2.Password_Hash = _passwordHasher.HashPassword(superAdmin2, "AdminMaster@2");

                // Adiciona os Super Admins ao banco
                _context.Administrators.Add(superAdmin1);
                _context.Administrators.Add(superAdmin2);

                // Salva as alterações no banco de dados
                _context.SaveChanges();
            }
        }
    }


}
