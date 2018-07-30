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
        public IActionResult CreateTripForCustomer(TripCreationDto tripCreationDto)
        {   
            var tripEntity = _mapper.Map<Trip>(tripCreationDto);

            tripEntity.CreationTime = DateTime.UtcNow;

            tripEntity.Id = Guid.NewGuid();

            tripEntity.CustomerId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value);

            _tripsCashe.SetTrip(tripEntity);

            return Ok();
        }

        [Authorize(Policy = "Customer")]
        [HttpDelete()]
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
        [HttpPut()]
        public IActionResult UpdateTripForCustomer(TripUpdateDto tripUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerid = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;

            if (customerid == null)
            {
                return BadRequest();
            }

            var tripToUpdate = _tripsCashe.GetTrip(Guid.Parse(customerid));

            if (tripToUpdate == null)
            {
                return NotFound();
            }

            _mapper.Map(tripUpdateDto, tripToUpdate);

            _tripsCashe.SetTrip(tripToUpdate);

            return Ok();
        }


        [Authorize(Policy = "Driver")]
        [HttpPost("taketrip")]
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

            return Ok();
        }
        
        
        [Authorize (Policy = "Driver")]
        [HttpPost("starttrip")]
        public IActionResult StartTrip(LatLonDto location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;
            

            
            return Ok();
        }   
    }
}
