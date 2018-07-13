using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Taxi.Data;
using Taxi.Entities;
using Taxi.Models;

namespace Taxi.Services
{
    public class UsersRepository : IUsersRepository
    {
        ApplicationDbContext _dataContext;
        private IMapper _mapper;
        private ApiUserManager _userManager;

        public UsersRepository(ApplicationDbContext dataContext, IMapper mapper, ApiUserManager userManager)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task AddCustomer(Customer customer)
        {
            await _dataContext.Customers.AddAsync(customer);
         
            await _dataContext.SaveChangesAsync();

            var claim = new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.CustomerAccess);

            var addClaimRes = await _userManager.AddClaimAsync(customer.Identity, claim);


        }

        public async Task AddDriver(Driver driver)
        {
            await _dataContext.Drivers.AddAsync(driver);
            await _dataContext.SaveChangesAsync();

            var claim = new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.DriverAccess);

            var addClaimRes = await _userManager.AddClaimAsync(driver.Identity, claim);

        }

        public Driver GetDriverByIdentityId(string identityId)
        {
            var driver = _dataContext.Drivers.Where(o => o.IdentityId == identityId).FirstOrDefault();

            return driver;
        }

        public Customer GetCustomerByIdentityId(string identityId)
        {
            var customer = _dataContext.Customers.Where(o => o.IdentityId == identityId).FirstOrDefault();

            return customer;
        }
        
        
        
    }
}
