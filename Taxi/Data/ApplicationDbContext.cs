using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;

namespace Taxi.Data
{
    public class ApplicationDbContext: IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions options): base (options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
            });
            base.OnModelCreating(builder);
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Driver> Drivers { get; set; }
    }
}
