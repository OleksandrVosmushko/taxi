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
   }
}
