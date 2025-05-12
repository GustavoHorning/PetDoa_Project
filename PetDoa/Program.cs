using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetDoa.Data;
using PetDoa.Models;
using PetDoa.Services.Interfaces;
using PetDoa.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text.Json.Serialization;
using PetDoa.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace PetDoa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            //builder.Services.AddControllers();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddScoped<IDonorService, DonorService>();
            builder.Services.AddScoped<IPasswordHasher<Donor>, PasswordHasher<Donor>>();
            builder.Services.AddScoped<IPasswordHasher<Administrator>, PasswordHasher<Administrator>>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<AdminSeeder>();
            builder.Services.AddScoped<IOngService, OngService>();
            builder.Services.AddScoped<IDonationService, DonationService>();
            builder.Services.AddSingleton<IFileStorageService, AzureBlobStorageService>();



      var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // true se usar HTTPS
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    RoleClaimType = ClaimTypes.Role
                };
            })
            .AddGoogle(options => // <-- ADICIONE ESTA CHAMADA .AddGoogle(...)
            {
                // Busca a seção de configuração "Authentication:Google"
                IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");

                // Valida se as chaves existem na configuração (boa prática)
                string? clientId = googleAuthNSection["ClientId"];
                string? clientSecret = googleAuthNSection["ClientSecret"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    throw new InvalidOperationException("Credenciais do Google (ClientId ou ClientSecret) não configuradas.");
                }

                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            }).AddCookie(IdentityConstants.ExternalScheme);
            //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            builder.Services.AddAuthorization();

            var app = builder.Build();
            app.UseExceptionHandler();

            // Criar o banco de dados e rodar o seeder durante a inicialização
            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    var seeder = services.GetRequiredService<AdminSeeder>();

            //    // Popula os administradores
            //    seeder.SeedSuperAdmins();
            //}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // 🔐 Ordem importa: Autenticação primeiro
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
