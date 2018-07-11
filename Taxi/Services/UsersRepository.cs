using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public UsersRepository(ApplicationDbContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }
        public async Task AddCustomer(Customer customer)
        {
            await _dataContext.Customers.AddAsync(customer);
            await _dataContext.SaveChangesAsync();
        }

        public async Task AddDriver(Driver driver)
        {
            await _dataContext.Drivers.AddAsync(driver);
            await _dataContext.SaveChangesAsync();
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
