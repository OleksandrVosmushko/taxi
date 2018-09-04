using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models;
using Taxi.Services;

namespace Taxi.Hubs
{
    public class RouteHub : Hub
    {

        private IUsersRepository _usersRepository;

        public RouteHub(IUsersRepository usersRepository)   {
            _usersRepository = usersRepository;
        }
        

        public async Task Send(string customerConnectionId, double lat, double lon)
        {
            if(!string.IsNullOrEmpty(customerConnectionId))
            {
                await Clients.Client(customerConnectionId).SendAsync("postGeoData", lat, lon);
            }          
        }

        [Authorize(Policy = "Customer")]
        public void ConnectCustomer()
        {
            var customerId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;
            var customer = _usersRepository.GetCustomerById(Guid.Parse(customerId));
            customer.ConnectionId = Context.ConnectionId;
            _usersRepository.UpdateCustomer(customer).Wait();
        }

        [Authorize(Policy = "Driver")]
        public void ConnectDriver()
        {          
            var driverId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;
            var driver = _usersRepository.GetDriverById(Guid.Parse( driverId));
            driver.ConnectionId = Context.ConnectionId;
            _usersRepository.UpdateDriver(driver).Wait();
        }


        public override Task OnDisconnectedAsync(Exception e)
        {
            var customerId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;
            var customer = _usersRepository.GetCustomerById(Guid.Parse( customerId));
            if (customer != null)
            {
                customer.ConnectionId = null;
                return base.OnDisconnectedAsync(e);
            }

            var driverId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;
            var driver = _usersRepository.GetDriverById(Guid.Parse( driverId));
            if (driver != null)
            {
                driver.ConnectionId = null;
                return base.OnDisconnectedAsync(e);
            }

            return base.OnDisconnectedAsync(e);
        }
    }
}
