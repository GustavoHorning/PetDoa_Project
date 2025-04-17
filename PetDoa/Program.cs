using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetDoa.Data;
using PetDoa.Models;
using PetDoa.Services.Interfaces;
using PetDoa.Services;

namespace PetDoa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddScoped<IDonorService, DonorService>();
            builder.Services.AddScoped<IPasswordHasher<Donor>, PasswordHasher<Donor>>();




            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
