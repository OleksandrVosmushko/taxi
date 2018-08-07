using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Taxi.Data;
using Taxi.Entities;

namespace Taxi.Helpers
{
    public class DataSeed
    {
        public static void Initialize(IServiceProvider serviceProvider, UserManager<AppUser> userManager)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
          //  context.Database.EnsureDeleted();
            //  context.Database.EnsureCreated();
            context.Database.Migrate();
            if (userManager.FindByNameAsync("1@mail.ru").Result == null)
            {
                var id = "d01ba177-4d21-4d75-94ca-5702ecda7487";
                var drID = new Guid("7c39a01a-abb3-406f-bbb1-7c63d79e8cdc");
                var cID = new Guid("e67e9875-f674-4120-b0ea-00dea51fa561");

                var user = new AppUser()
                {
                    FirstName = "1",
                    LastName = "1",
                    Email = "1@mail.ru",
                    PhoneNumber = "111",
                    UserName = "1@mail.ru",
                    EmailConfirmed = true,
                    Id = id
                };
                IdentityResult result = userManager.CreateAsync
                (user, "111111").Result;
                var Driver = new Driver()
                {
                    Id = drID,
                    City = "Kyiv",
                    IdentityId = id,
                    Vehicle = new Vehicle()
                    {
                        Brand = "BMW",
                        Model = "M3",
                        Color = "black",
                        Number = "777"
                    }
                };
                context.Drivers.Add(Driver);
             
                var c = new Customer()
                {
                    Id = cID,
                    IdentityId = id
                };
                context.Customers.Add(c);
                context.SaveChanges();
                var claims = new List<Claim> {
                    new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.DriverAccess),
                    new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId, Driver.Id.ToString()),
                    new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId, c.Id.ToString())
                 };

                var addClaimRes = userManager.AddClaimsAsync(Driver.Identity, claims).Result;
            }
            if (userManager.FindByNameAsync("2@mail.ru").Result == null)
            {
                var id = "83ea2640-5ed2-4afe-8f86-c29450333c1b";
                var drID = new Guid("8202c164-9e5a-417f-a14c-e7cf6215d898");
                var user = new AppUser()
                {
                    FirstName = "2",
                    LastName = "2",
                    Email = "2@mail.ru",
                    PhoneNumber = "222",
                    UserName = "2@mail.ru",
                    EmailConfirmed = true,
                    Id = id
                };
                IdentityResult result = userManager.CreateAsync
                (user, "111111").Result;
                var c = new Customer()
                {
                    Id = drID,
                    IdentityId = id 
                };
                context.Customers.Add(c);

                context.SaveChanges();

                var claims = new List<Claim> {
                    new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.CustomerAccess),
                    new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId, c.Id.ToString())
                };

                var addClaimRes = userManager.AddClaimsAsync(c.Identity, claims).Result;
            }


        }
    }
}
