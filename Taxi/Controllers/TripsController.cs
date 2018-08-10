﻿using AutoMapper;
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
        private ITripsRepository _tripsRepo;
        private IMapper _mapper;
        private IUsersRepository _usersRepository;

        public TripsController(IMapper mapper,
            ITripsRepository tripsRepo,
            IUsersRepository usersRepository)
        {
            _tripsRepo = tripsRepo;
            _mapper = mapper;
            _usersRepository = usersRepository;
        }
        [Authorize(Policy = "Customer")]
        [HttpPost()]
        [ProducesResponseType(204)]
        public IActionResult CreateTripForCustomer([FromBody]TripCreationDto tripCreationDto)
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

            _tripsRepo.SetTrip(tripEntity);

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

            _tripsRepo.RemoveTrip(Guid.Parse(customerid));

            return NoContent();
        }
        //ToDo : check if no exceptions
        [Authorize(Policy = "Customer")]
        [HttpPut("from")]
        [ProducesResponseType(204)]
        public IActionResult UpdateTripStartLocation([FromBody]LatLonDto location)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var customerId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value);

            var res = _tripsRepo.UpdateTripLocation(location.Longitude, location.Latitude, customerId);

            if (res == false)
                return BadRequest();

            return NoContent();
        }
        
        [HttpGet()]
        [Authorize(Policy = "Driver")]
        [ProducesResponseType(200)]
        public IActionResult GetNearTrips(LatLonDto driverLocation)
        {
            return Ok(_tripsRepo.GetNearTrips(driverLocation.Longitude, driverLocation.Latitude));
        }

        [Authorize(Policy = "Driver")]
        [HttpPost("taketrip")]
        [ProducesResponseType(204)]
        public IActionResult AddDriverToTrip(Guid customerId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trip = _tripsRepo.GetTrip(customerId);
           
            if (trip == null || driverId == null || trip.DriverId != null) 
                return BadRequest();

            trip.DriverId = Guid.Parse(driverId);

            trip.DriverTakeTripTime = DateTime.UtcNow;

            _tripsRepo.SetTrip(trip);

            return NoContent();
        }
        
        [Authorize (Policy = "Driver")]
        [HttpPost("starttrip")]
        [ProducesResponseType(200)]
        public IActionResult StartTrip([FromBody]LatLonDto location)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trip = _tripsRepo.GetTripByDriver(Guid.Parse(driverId));

            if (trip == null)
                return BadRequest();

            trip.StartTime = DateTime.UtcNow;

            var startpoint = trip.Places.FirstOrDefault(p => p.IsFrom == true);

            startpoint.Latitude = location.Latitude;

            startpoint.Longitude = location.Longitude;

            _tripsRepo.SetTrip(trip);

            var from = trip.Places.FirstOrDefault(p => p.IsFrom == true);
            var to = trip.Places.FirstOrDefault(p => p.IsTo == true);
            var customer = _usersRepository.GetCustomerById(trip.CustomerId);
            var toReturn = new TripDto()
            {

                CustomerId = trip.CustomerId,
                From = new PlaceDto
                {
                    Longitude = from.Longitude,
                    Latitude = from.Latitude
                },
                To = new PlaceDto
                {
                    Longitude = to.Longitude,
                    Latitude = to.Latitude
                },
                FirstName = customer.Identity.FirstName,
                LastName = customer.Identity.LastName                
            };

            return Ok(toReturn);
        }  

        [Authorize(Policy = "Driver")]
        [HttpPost("finishtrip")]
        public async Task<IActionResult> FinishTripAsync([FromBody]FinishTripDto finishTrip)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trip = _tripsRepo.GetTripByDriver(Guid.Parse(driverId));

            if (trip == null)
                return BadRequest();

            trip.FinishTime = DateTime.UtcNow;

            var finishPlace = trip.Places.FirstOrDefault(p => p.IsTo == true);

            finishPlace.Latitude = finishTrip.To.Latitude;

            finishPlace.Longitude = finishTrip.To.Longitude;

            var tripHistory = _mapper.Map<TripHistory>(trip);

            var places = new List<FinishTripPlace>();

            foreach(var place in trip.Places)
            {
                places.Add(_mapper.Map<FinishTripPlace>(place));
            }
            tripHistory.Places = places;

            tripHistory.Price = finishTrip.Price;

            await _tripsRepo.AddTripHistory(tripHistory);

            _tripsRepo.RemoveTrip(trip.CustomerId);

            var from = tripHistory.Places.FirstOrDefault(p => p.IsFrom == true);
            var to = tripHistory.Places.FirstOrDefault(p => p.IsTo == true);

            var toReturn = new TripHistoryDto()
            {
                CustomerId = tripHistory.CustomerId,
                DriverId = tripHistory.DriverId,

                Id = tripHistory.Id,
                From = new PlaceDto
                {
                    Longitude = from.Longitude,
                    Latitude = from.Latitude
                },
                To = new PlaceDto
                {
                    Longitude = to.Longitude,
                    Latitude = to.Latitude
                },
                FinishTime = tripHistory.FinishTime,
                Price = tripHistory.Price                
            };//check if correctly maps from nullable
            return Ok(toReturn);
        }
    }
}
