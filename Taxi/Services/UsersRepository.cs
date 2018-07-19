using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private UserManager<AppUser> _userManager;

        public UsersRepository(ApplicationDbContext dataContext, IMapper mapper, UserManager<AppUser> userManager)
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

        public async Task UpdateCustomer(Customer customer)
        {
            await _dataContext.SaveChangesAsync();
        }

        public async Task UpdateDriver(Driver Driver)
        {
            await _dataContext.SaveChangesAsync();
        }

        public async Task AddDriver(Driver driver)
        {
            await _dataContext.Drivers.AddAsync(driver);
            await _dataContext.SaveChangesAsync();

            var claims = new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.DriverAccess);
            
            var addClaimRes = await _userManager.AddClaimAsync(driver.Identity, claims);
            
        }

        public Driver GetDriverByIdentityId(string identityId)
        {
            var driver = _dataContext.Drivers.Where(o => o.IdentityId == identityId).Include(d => d.Identity).FirstOrDefault();

            return driver;
        }

        public Customer GetCustomerByIdentityId(string identityId)
        {
            var customer = _dataContext.Customers.Where(o => o.IdentityId == identityId).Include(d => d.Identity).FirstOrDefault();

            return customer;
        }
        
        public Customer GetCustomerById(Guid id)
        {
            var customer = _dataContext.Customers.Include(d => d.Identity).SingleOrDefault(o => o.Id == id);

            return customer;
        }

        public Driver GetDriverById(Guid id)
        {
            var driver = _dataContext.Drivers.Include(d => d.Identity).SingleOrDefault(o => o.Id == id);

            return driver;
        }

        public IEnumerable<Driver> GetDrivers()
        {
            return _dataContext.Drivers.ToList();
        }

        public IEnumerable<Customer> GetCustomers()
        {
            return _dataContext.Customers.ToList();
        }

        public RefreshToken GetRefreshToken(string token)
        {
            return _dataContext.RefreshTokens.FirstOrDefault(t => t.Token == token);
        }

        public async Task<bool> DeleteRefleshToken(RefreshToken token)
        {
            try
            {
                 _dataContext.RefreshTokens.Remove(token);
                await _dataContext.SaveChangesAsync();

            } catch (Exception e)
            {
                return false;
            }
            return true;
        }
        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
            try
            {
                await _dataContext.RefreshTokens.AddAsync(token);
                await _dataContext.SaveChangesAsync();

            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public IEnumerable<RefreshToken> GetTokensForUser(string userId)
        {
            return _dataContext.RefreshTokens.Where(t => t.IdentityId == userId).ToList();
        }
    }
}
