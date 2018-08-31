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

        //public RouteHub() { }

        public RouteHub(IUsersRepository usersRepository) {
            _usersRepository = usersRepository;
        }
        

        public async Task Send(string customerConnectionId, double lat, double lon)
        {
            if(customerConnectionId != "")
            {
                await Clients.Client(customerConnectionId).SendAsync("postGeoData", lat, lon);
            }          
        }

        //[Authorize(Policy = "Customer")]
        public void ConnectCustomer()
        {
            var customerId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;
            var customer = _usersRepository.GetCustomerByConnectionId(customerId);
            customer.ConnectionId = Context.ConnectionId;
            _usersRepository.UpdateCustomer(customer);
        }

        //[Authorize(Policy = "Driver")]
        public void ConnectDriver()
        {          
            var driverId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;
            var driver = _usersRepository.GetDriverByIdentityId(driverId);
            driver.ConnectionId = Context.ConnectionId;
            _usersRepository.UpdateDriver(driver);
        }


        public override Task OnDisconnectedAsync(Exception e)
        {
            var customerId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;
            var customer = _usersRepository.GetCustomerByConnectionId(customerId);
            if (customer != null)
            {
                customer.ConnectionId = "";
                return base.OnDisconnectedAsync(e);
            }

            var driverId = Context.User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;
            var driver = _usersRepository.GetDriverByIdentityId(driverId);
            if (driver != null)
            {
                driver.ConnectionId = "";
                return base.OnDisconnectedAsync(e);
            }

            return base.OnDisconnectedAsync(e);
        }
    }
}
