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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trip>()
            .HasMany(c => c.Places)
            .WithOne(a => a.Trip)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TripHistory>()
                .HasMany(c => c.RouteNodes)
                .WithOne(a => a.TripHistory)
                .OnDelete(DeleteBehavior.Cascade);


            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Driver> Drivers { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Trip> Trips { get; set; }

        public DbSet<Place> Places { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Picture> Pictures { get; set; }

        public DbSet<ProfilePicture> ProfilePictures { get; set; }

        public DbSet<TripHistory> TripHistories { get; set; }

        public DbSet<FinishTripPlace> FinishTripPlaces { get; set; }

        public DbSet<TripRouteNode> TripRouteNodes { get; set; }
    }
}
