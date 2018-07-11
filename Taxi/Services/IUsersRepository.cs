using System;
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


        Customer GetCustomerByIdentityId(string identityId);

        Driver GetDriverByIdentityId(string identityId);
    }
}
