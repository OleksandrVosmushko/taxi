using Amazon.S3;
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
using Taxi.Helpers;
using Taxi.Models;
using Taxi.Models.Admins;
using Taxi.Models.Drivers;

namespace Taxi.Services
{
    public class UsersRepository : IUsersRepository
    {
        ApplicationDbContext _dataContext;
        private IMapper _mapper;
        private UserManager<AppUser> _userManager;
        private IUploadService _uploadService;

        public UsersRepository(ApplicationDbContext dataContext,
            IMapper mapper, 
            UserManager<AppUser> userManager,
            IUploadService uploadService)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _userManager = userManager;
            _uploadService = uploadService;
        }


        public async Task RemoveUser(AppUser user)
        {
            var driver = _dataContext.Drivers.FirstOrDefault(d => d.IdentityId == user.Id);
            var customer = _dataContext.Customers.FirstOrDefault(d => d.IdentityId == user.Id);
            if (driver != null)
                _dataContext.Remove(driver);
            if (customer != null)
                _dataContext.Remove(customer);
            await _dataContext.SaveChangesAsync();
            await _userManager.DeleteAsync(user);
        }

        public AppUser GetUser(string id)
        {
            return _dataContext.Users.Include(u => u.ProfilePicture).FirstOrDefault(ur => ur.Id == id);
        }

        public PagedList<RefundRequest> GetRefundRequests(RefundResourceParameters resourceParameters)
        {
            IQueryable<RefundRequest> beforePaging = _dataContext.RefundRequests;

            if (resourceParameters.IsSolved != null)
            {
                beforePaging = beforePaging.Where(p => p.Solved == resourceParameters.IsSolved);
            }
            return PagedList<RefundRequest>.Create(beforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public PagedList<DriverLicense> GetDriverLicenses(DriverLicenseResourceParameters resourceParameters)
        {
            IQueryable<DriverLicense> beforePaging = _dataContext.DriverLicenses;

            if (resourceParameters.IsApproved != null)
            {
                beforePaging = beforePaging.Where(p => p.IsApproved == resourceParameters.IsApproved);
            }

            return PagedList<DriverLicense>.Create(beforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);

        }

        public async Task AddAdminResponse(AdminResponse response)
        {
            await _dataContext.AdminResponces.AddAsync(response);

            await _dataContext.SaveChangesAsync();
        }

        public async Task<PagedList<AppUser>> GetUsers(UserResourceParameters paginationParameters)
        {
            var beforePaging = //(!string.IsNullOrEmpty(paginationParameters.Rol))?
              //  await _userManager.GetUsersForClaimAsync(new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, paginationParameters.Rol)) :
                _userManager.Users;
            

            if (!string.IsNullOrEmpty(paginationParameters.SearchQuery))
            {
                var searchForWhereClause = paginationParameters.SearchQuery.Trim().ToLowerInvariant();
                 
                beforePaging = beforePaging.Where(a => (a.FirstName + " "+ a.LastName + " " + a.Email + " "+ a.PhoneNumber).ToLowerInvariant().Contains(searchForWhereClause));
            }
            
            if (paginationParameters.EmailConfirmed != null)
            {
                beforePaging = beforePaging.Where(u => u.EmailConfirmed == paginationParameters.EmailConfirmed );
            }

            if (!string.IsNullOrEmpty(paginationParameters.Rol))
            {
                beforePaging = beforePaging.Where(u =>
                    _dataContext.UserClaims.FirstOrDefault(c =>
                        c.UserId == u.Id && c.ClaimValue == paginationParameters.Rol) != null);
            }

            return PagedList<AppUser>.Create(beforePaging.Include(u => u.ProfilePicture), paginationParameters.PageNumber, paginationParameters.PageSize);
        }

        public Admin GetAdminById(Guid adminId)
        {
            return _dataContext.Admins.Include(a => a.Identity).ThenInclude(a => a.ProfilePicture).FirstOrDefault(ad => ad.Id == adminId);
        }

        public PagedList<Admin> GetAdmins(PaginationParameters paginationParameters)
        {
            var beforePaging = _dataContext.Admins.Include(a => a.Identity).ThenInclude(i => i.ProfilePicture);
            return PagedList<Admin>.Create(beforePaging, paginationParameters.PageNumber, paginationParameters.PageSize);
        }

        public async Task AddAdmin(Admin admin)
        {
            await _dataContext.Admins.AddAsync(admin);

            var claims = new List<Claim> {
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.AdminId, admin.Id.ToString())
            };
            var identity = await _userManager.FindByIdAsync(admin.IdentityId);

            var addClaimRes = await _userManager.AddClaimsAsync(identity, claims);

            if (admin.IsApproved == true)
                await ApproveAdmin(admin);
        }

        public async Task ApproveAdmin(Admin admin)
        {
            var claims = new List<Claim> {
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol, Helpers.Constants.Strings.JwtClaims.AdminAccess),
            };
            var identity = await _userManager.FindByIdAsync(admin.IdentityId);

            var addClaimRes = await _userManager.AddClaimsAsync(identity, claims);
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
            var customer = _dataContext.Customers.Include(d => d.Identity).Include(c=>c.CurrentTrip).SingleOrDefault(o => o.Id == id);

            return customer;
        }

        public Driver GetDriverById(Guid id)
        {
            var driver = _dataContext.Drivers.Include(d => d.Identity)
                .Include(dv => dv.DriverLicense)
                .Include(dr => dr.Vehicle)
                .ThenInclude(v=> v.Pictures)
                .SingleOrDefault(o => o.Id == id);

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

        public async Task<bool> AddVehicleToDriver(Guid DriverId, Vehicle vehicle)
        {
            try
            {
                var driver = GetDriverById(DriverId);
                if (driver == null)
                    return false;
                if (driver.Vehicle != null)
                    await RemoveVehicle(driver.Vehicle);
                driver.Vehicle = vehicle;
                await _dataContext.SaveChangesAsync();
            } catch
            {
                return false;
            }
            return true;
        }

        public async Task RemoveVehicle(Vehicle vehicle)
        {
            foreach (var p in vehicle.Pictures)
            {
                await _uploadService.DeleteObjectAsync(p.Id);
            }
            _dataContext.Remove(vehicle);

            await _dataContext.SaveChangesAsync();
        }
        
        public async Task<Vehicle> GetVehicle(Guid vehicleId)
        {
            return await _dataContext.Vehicles.Include(o => o.Pictures).FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task AddPictureToVehicle(Vehicle v, string id)
        {
            v.Pictures.Add(new Picture() { Id = id });
            await _dataContext.SaveChangesAsync(); 
        }  

        public async Task<bool> RemoveProfilePicture(AppUser user)
        {
            await _uploadService.DeleteObjectAsync(user.ProfilePicture.Id);

            _dataContext.ProfilePictures.Remove(user.ProfilePicture);

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddProfilePicture(AppUser user, ProfilePicture picture)
        {
            user.ProfilePicture = picture;

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveVehicleImage(Driver driver, string imageId)
        {
            var res = driver.Vehicle.Pictures.RemoveAll(p => p.Id == imageId);

            if (res == 0)
                return false;

            await _uploadService.DeleteObjectAsync(imageId);

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddDriverLicense(DriverLicense driverLicense)
        {
            await _dataContext.DriverLicenses.AddAsync(driverLicense);

            await _dataContext.SaveChangesAsync();

            return true;
        }

        public async Task UpdateDriverLicense(DriverLicense driverLicense)
        {
            await _dataContext.SaveChangesAsync();
        }

        public PagedList<AdminResponse> GetAdminResponses(string id, PaginationParameters resourceParameters)
        {
            var beforePaging = _dataContext.AdminResponces.Where(a => a.IdentityId == id).OrderByDescending(ar => ar.CreationTime);

            return PagedList<AdminResponse>.Create(beforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public async Task<bool> RemoveDriverLicense(DriverLicense license)
        {
            _dataContext.DriverLicenses.Remove(license);

            await _uploadService.DeleteObjectAsync(license.ImageId);

            await _dataContext.SaveChangesAsync();
            
            return true;
        }
    }
}
