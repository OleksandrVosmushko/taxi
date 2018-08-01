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

            var claims = new List<Claim> {
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.CustomerAccess),
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId, customer.Id.ToString())
            };

            var addClaimRes = await _userManager.AddClaimsAsync(customer.Identity, claims);
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

            var claims = new List<Claim> {
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.DriverAccess),
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId, driver.Id.ToString())
            };
            
            var addClaimRes = await _userManager.AddClaimsAsync(driver.Identity, claims);
            
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

        public async Task<bool> AddVehicleToDriver(Vehicle vehicle)
        {
            try
            {
                await _dataContext.Vehicles.AddAsync(vehicle);
                await _dataContext.SaveChangesAsync();
            } catch
            {
                return false;
            }
            return true;
        }

        public async Task RemoveVehicle(Vehicle vehicle)
        {
            _dataContext.Remove(vehicle);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Vehicle> GetVehicle(Guid vehicleId)
        {
            return await _dataContext.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
        }
    }
}
