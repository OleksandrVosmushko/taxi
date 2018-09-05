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
        public async Task ConnectCustomer()
        {
            var customerId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;
            var customer = _usersRepository.GetCustomerById(Guid.Parse(customerId));
            customer.ConnectionId = Context.ConnectionId;
            await _usersRepository.UpdateCustomer(customer);
        }

        [Authorize(Policy = "Driver")]
        public async Task ConnectDriver()
        {          
            var driverId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;
            var driver = _usersRepository.GetDriverById(Guid.Parse( driverId));
            driver.ConnectionId = Context.ConnectionId;
            await _usersRepository.UpdateDriver(driver);
        }


        public override async Task OnDisconnectedAsync(Exception e)
        {
            //var customerId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;
            var connId = Context.ConnectionId;
            var customer = _usersRepository.GetCustomerByConnectionId( connId);
            if (customer != null)
            {
                customer.ConnectionId = null;
                await _usersRepository.UpdateCustomer(customer);
            }
            
            var driver = _usersRepository.GetDriverByConnectionId( connId);
            if (driver != null)
            {
                driver.ConnectionId = null;
                await _usersRepository.UpdateDriver(driver);            }

            await base.OnDisconnectedAsync(e);
        }
    }
}
