using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models;
using Taxi.Models.Trips;
using Taxi.Services;

namespace Taxi.Controllers
{
    [Route("api/[controller]")]
    public class TripsController : Controller
    {
        private ITripsRepository _tripsCashe;
        private IMapper _mapper;

        public TripsController(IMapper mapper,
            ITripsRepository tripsCashe)
        {
            _tripsCashe = tripsCashe;
            _mapper = mapper;
        }
        [Authorize(Policy = "Customer")]
        [HttpPost()]
        [ProducesResponseType(204)]
        public IActionResult CreateTripForCustomer(TripCreationDto tripCreationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tripEntity = new Trip()
            {
                CreationTime = DateTime.UtcNow,
                CustomerId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value),
                Places = new List<Place>
                {
                    new Place()
                    {
                        Latitude = tripCreationDto.From.Latitude,
                        Longitude = tripCreationDto.From.Longitude ,
                        IsFrom = true
                    },
                    new Place()
                    {
                        Latitude = tripCreationDto.To.Latitude,
                        Longitude = tripCreationDto.To.Longitude,
                        IsTo = true
                    }
                }
            };

            _tripsCashe.SetTrip(tripEntity);

            return NoContent();
        }

        [Authorize(Policy = "Customer")]
        [HttpDelete()]
        [ProducesResponseType(204)]
        public IActionResult DeleteTripForCustomer()
        {
            var customerid = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;

            if (customerid == null)
            {
                return BadRequest();
            }

            _tripsCashe.RemoveTrip(Guid.Parse(customerid));

            return NoContent();
        }

        [Authorize(Policy = "Customer")]
        [HttpPut("from")]
        [ProducesResponseType(204)]
        public IActionResult UpdateTripStartLocation(LatLonDto location)
        {
            var customerId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value);

            var res = _tripsCashe.UpdateTripLocation(location.Longitude, location.Latitude, customerId);

            if (res == false)
                return BadRequest();

            return NoContent();
        }


        [Authorize(Policy = "Driver")]
        [HttpPost("taketrip")]
        [ProducesResponseType(204)]
        public IActionResult AddDriverToTrip(Guid customerId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trip = _tripsCashe.GetTrip(customerId);
           
            if (trip == null || driverId == null || trip.DriverId != null) 
                return BadRequest();

            trip.DriverId = Guid.Parse(driverId);

            trip.DriverTakeTripTime = DateTime.UtcNow;

            _tripsCashe.SetTrip(trip);

            return NoContent();
        }
        
        
        
        [Authorize (Policy = "Driver")]
        [HttpPost("starttrip")]
        [ProducesResponseType(204)]
        public IActionResult StartTrip(LatLonDto location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;
            
            return NoContent();
        }   
    }
}
