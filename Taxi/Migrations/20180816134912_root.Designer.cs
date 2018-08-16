﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using Taxi.Data;

namespace Taxi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180816134912_root")]
    partial class root
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Taxi.Entities.Admin", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("IdentityId");

                    b.Property<bool>("IsApproved");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.ToTable("Admins");
                });

            modelBuilder.Entity("Taxi.Entities.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Taxi.Entities.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("IdentityId");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("Taxi.Entities.Driver", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City");

                    b.Property<string>("IdentityId");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId");

                    b.ToTable("Drivers");
                });

            modelBuilder.Entity("Taxi.Entities.DriverLicense", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("DriverId");

                    b.Property<string>("ImageId");

                    b.Property<DateTime>("LicensedFrom");

                    b.Property<DateTime>("LicensedTo");

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasIndex("DriverId")
                        .IsUnique();

                    b.ToTable("DriverLicenses");
                });

            modelBuilder.Entity("Taxi.Entities.FinishTripPlace", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsFrom");

                    b.Property<bool>("IsTo");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<Guid>("TripHistoryId");

                    b.HasKey("Id");

                    b.HasIndex("TripHistoryId");

                    b.ToTable("FinishTripPlaces");
                });

            modelBuilder.Entity("Taxi.Entities.Picture", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("VehicleId");

                    b.HasKey("Id");

                    b.HasIndex("VehicleId");

                    b.ToTable("Pictures");
                });

            modelBuilder.Entity("Taxi.Entities.Place", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsFrom");

                    b.Property<bool>("IsTo");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<Guid>("TripId");

                    b.HasKey("id");

                    b.HasIndex("TripId");

                    b.ToTable("Places");
                });

            modelBuilder.Entity("Taxi.Entities.ProfilePicture", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("IdentityId");

                    b.HasKey("Id");

                    b.HasIndex("IdentityId")
                        .IsUnique();

                    b.ToTable("ProfilePictures");
                });

            modelBuilder.Entity("Taxi.Entities.RefreshToken", b =>
                {
                    b.Property<string>("Token")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Expiration");

                    b.Property<string>("IdentityId");

                    b.Property<string>("Ip");

                    b.Property<string>("Useragent");

                    b.HasKey("Token");

                    b.HasIndex("IdentityId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Taxi.Entities.Trip", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<Guid>("CustomerId");

                    b.Property<double>("Distance");

                    b.Property<Guid?>("DriverId");

                    b.Property<DateTime>("DriverTakeTripTime");

                    b.Property<DateTime>("FinishTime");

                    b.Property<double>("LastLat");

                    b.Property<double>("LastLon");

                    b.Property<DateTime>("LastUpdateTime");

                    b.Property<DateTime>("StartTime");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId")
                        .IsUnique();

                    b.HasIndex("DriverId")
                        .IsUnique();

                    b.ToTable("Trips");
                });

            modelBuilder.Entity("Taxi.Entities.TripHistory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationTime");

                    b.Property<Guid>("CustomerId");

                    b.Property<double>("Distance");

                    b.Property<Guid>("DriverId");

                    b.Property<DateTime>("DriverTakeTripTime");

                    b.Property<DateTime>("FinishTime");

                    b.Property<decimal>("Price");

                    b.Property<DateTime>("StartTime");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("DriverId");

                    b.ToTable("TripHistories");
                });

            modelBuilder.Entity("Taxi.Entities.TripRouteNode", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<Guid?>("TripHistoryId");

                    b.Property<Guid?>("TripId");

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasIndex("TripHistoryId");

                    b.HasIndex("TripId");

                    b.ToTable("TripRouteNodes");
                });

            modelBuilder.Entity("Taxi.Entities.Vehicle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Brand");

                    b.Property<string>("Color");

                    b.Property<Guid>("DriverId");

                    b.Property<string>("Model");

                    b.Property<string>("Number");

                    b.HasKey("Id");

                    b.HasIndex("DriverId")
                        .IsUnique();

                    b.ToTable("Vehicles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Taxi.Entities.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Taxi.Entities.Admin", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId");
                });

            modelBuilder.Entity("Taxi.Entities.Customer", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId");
                });

            modelBuilder.Entity("Taxi.Entities.Driver", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser", "Identity")
                        .WithMany()
                        .HasForeignKey("IdentityId");
                });

            modelBuilder.Entity("Taxi.Entities.DriverLicense", b =>
                {
                    b.HasOne("Taxi.Entities.Driver", "Driver")
                        .WithOne("DriverLicense")
                        .HasForeignKey("Taxi.Entities.DriverLicense", "DriverId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Taxi.Entities.FinishTripPlace", b =>
                {
                    b.HasOne("Taxi.Entities.TripHistory", "TripHistory")
                        .WithMany("Places")
                        .HasForeignKey("TripHistoryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Taxi.Entities.Picture", b =>
                {
                    b.HasOne("Taxi.Entities.Vehicle", "Vehicle")
                        .WithMany("Pictures")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Taxi.Entities.Place", b =>
                {
                    b.HasOne("Taxi.Entities.Trip", "Trip")
                        .WithMany("Places")
                        .HasForeignKey("TripId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Taxi.Entities.ProfilePicture", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser", "Identity")
                        .WithOne("ProfilePicture")
                        .HasForeignKey("Taxi.Entities.ProfilePicture", "IdentityId");
                });

            modelBuilder.Entity("Taxi.Entities.RefreshToken", b =>
                {
                    b.HasOne("Taxi.Entities.AppUser", "Identity")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("IdentityId");
                });

            modelBuilder.Entity("Taxi.Entities.Trip", b =>
                {
                    b.HasOne("Taxi.Entities.Customer", "Customer")
                        .WithOne("CurrentTrip")
                        .HasForeignKey("Taxi.Entities.Trip", "CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Taxi.Entities.Driver", "Driver")
                        .WithOne("CurrentTrip")
                        .HasForeignKey("Taxi.Entities.Trip", "DriverId");
                });

            modelBuilder.Entity("Taxi.Entities.TripHistory", b =>
                {
                    b.HasOne("Taxi.Entities.Customer", "Customer")
                        .WithMany("TripHistories")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Taxi.Entities.Driver", "Driver")
                        .WithMany("TripHistories")
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Taxi.Entities.TripRouteNode", b =>
                {
                    b.HasOne("Taxi.Entities.TripHistory", "TripHistory")
                        .WithMany("RouteNodes")
                        .HasForeignKey("TripHistoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Taxi.Entities.Trip", "Trip")
                        .WithMany("RouteNodes")
                        .HasForeignKey("TripId");
                });

            modelBuilder.Entity("Taxi.Entities.Vehicle", b =>
                {
                    b.HasOne("Taxi.Entities.Driver", "Driver")
                        .WithOne("Vehicle")
                        .HasForeignKey("Taxi.Entities.Vehicle", "DriverId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
