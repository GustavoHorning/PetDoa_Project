using PetDoa.Models;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using PetDoa.Models;

namespace PetDoa.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<ONG> ONGs { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Product> Products { get; set; }

  }
}
