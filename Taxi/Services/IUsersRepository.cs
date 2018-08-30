using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Helpers;
using Taxi.Models;
using Taxi.Models.Admins;
using Taxi.Models.Drivers;

namespace Taxi.Services
{
    public interface IUsersRepository
    {
        Task RemoveUser(AppUser user);

        AppUser GetUser(string id);
        PagedList<RefundRequest> GetRefundRequests(RefundResourceParameters resourceParameters);
        void UpdateRefund(RefundRequest request);
        RefundRequest GetRefundRequest(Guid id);
        PagedList<DriverLicense> GetDriverLicenses(DriverLicenseResourceParameters resourceParameters); 
        Task AddAdminResponse(AdminResponse response);
        Task<PagedList<AppUser>> GetUsers(UserResourceParameters resourceParameters);
        Admin GetAdminById(Guid adminId);

        PagedList<Admin> GetAdmins(PaginationParameters parameters);

        Task AddAdmin(Admin admin);

        Task ApproveAdmin(Admin admin);

        Task AddCustomer(Customer customer);

        Task AddDriver(Driver driver);

        Task UpdateCustomer(Customer customer);

        Task UpdateDriver(Driver driver);

        Customer GetCustomerByIdentityId(string identityId);

        Driver GetDriverByIdentityId(string identityId);

        Customer GetCustomerById(Guid Id);

        Driver GetDriverById(Guid Id);

        Customer GetCustomerByConnectionId(string connectionId);

        Driver GetDriverByConnectionId(string connectionId);

        IEnumerable<Driver> GetDrivers();

        IEnumerable<Customer> GetCustomers();

        RefreshToken GetRefreshToken(string token);

        Task<bool> DeleteRefleshToken(RefreshToken token);

        Task<bool> AddRefreshToken(RefreshToken token);

        IEnumerable<RefreshToken> GetTokensForUser(string userId);

        Task<bool> AddVehicleToDriver(Guid id, Vehicle vehicle);

        Task<Vehicle> GetVehicle(Guid vehicleId);

        Task RemoveVehicle(Vehicle vehicle);

        Task AddPictureToVehicle(Vehicle v,string id);

        Task<bool> RemoveProfilePicture(AppUser user);

        Task<bool> AddProfilePicture(AppUser user, ProfilePicture picture);

        Task<bool> RemoveVehicleImage(Driver driver, string imageId);

        Task<bool> RemoveDriverLicense(DriverLicense license);
        
        Task<bool> AddDriverLicense(DriverLicense driverLicense);

        Task UpdateDriverLicense(DriverLicense driverLicense);
        PagedList<AdminResponse> GetAdminResponses(string id, PaginationParameters resourceParameters);
    }
}
