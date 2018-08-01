﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models;

namespace Taxi.Services
{
    public interface IUsersRepository
    {
        Task AddCustomer(Customer customer);

        Task AddDriver(Driver driver);

        Task UpdateCustomer(Customer customer);

        Task UpdateDriver(Driver driver);

        Customer GetCustomerByIdentityId(string identityId);

        Driver GetDriverByIdentityId(string identityId);

        Customer GetCustomerById(Guid Id);

        Driver GetDriverById(Guid Id);

        IEnumerable<Driver> GetDrivers();

        IEnumerable<Customer> GetCustomers();

        RefreshToken GetRefreshToken(string token);

        Task<bool> DeleteRefleshToken(RefreshToken token);

        Task<bool> AddRefreshToken(RefreshToken token);

        IEnumerable<RefreshToken> GetTokensForUser(string userId);

        Task<bool> AddVehicleToDriver(Vehicle vehicle);

        Task<Vehicle> GetVehicle(Guid vehicleId);

        Task RemoveVehicle(Vehicle vehicle);
   }
}
