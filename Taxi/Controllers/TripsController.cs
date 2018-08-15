﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Helpers;
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
        private IUrlHelper _urlHelper;

        public TripsController(IMapper mapper,
            ITripsRepository tripsRepo,
            IUsersRepository usersRepository,
            IUrlHelper urlHelper)
        {
            _tripsRepo = tripsRepo;
            _mapper = mapper;
            _usersRepository = usersRepository;
            _urlHelper = urlHelper;
        }
        
        [Authorize(Policy = "Driver")]
        [HttpPost("updateroute")]
        public async Task<IActionResult> UpdateTripRoute([FromBody] LatLonDto latLon)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = Guid.Parse(User.Claims
                .FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value);

            var trip = _tripsRepo.GetTripByDriver(driverId);

            if (trip == null)
                return NotFound();

            if (trip.StartTime == default(DateTime))
            {
                ModelState.AddModelError(nameof(Trip), "Trip not started.");
                return BadRequest(ModelState);
            }
            var delta = Helpers.Location.CalculateKilometersDistance(trip.LastLat, trip.LastLon, latLon.Latitude, latLon.Longitude);

            if (Math.Abs(delta) > 0.001) //traveled 1+ meters
            {
                var node = _mapper.Map<TripRouteNode>(latLon);

                node.UpdateTime = DateTime.UtcNow;

                node.TripId = trip.Id;

                await _tripsRepo.AddNode(node);

                trip.Distance += delta;

                trip.LastLat = latLon.Latitude;

                trip.LastLon = latLon.Longitude;

                trip.LastUpdateTime = node.UpdateTime;

                await _tripsRepo.UpdateTrip(trip);

            }
            
            var from = trip.Places.FirstOrDefault(p => p.IsFrom == true);
            var to = trip.Places.FirstOrDefault(p => p.IsTo == true);


            var tripToReturn = new UpdateResultTripDto()
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
                LastUpdatePoint = new PlaceDto()
                {
                    Longitude = trip.LastLon,
                    Latitude = trip.LastLat
                },
                TraveledDistance = trip.Distance
            };

            return Ok(tripToReturn);
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

        [Authorize(Policy = "Customer")]
        [HttpGet("customer/tripstatus")]
        public IActionResult GetTripStatus()
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;

            var trip = _tripsRepo.GetTrip(Guid.Parse(customerId));

            if (trip == null)
                return NotFound();

            var tripStatusDto = Mapper.Map<TripStatusDto>(trip);

            var from = trip.Places.FirstOrDefault(p => p.IsFrom == true);
            var to = trip.Places.FirstOrDefault(p => p.IsTo == true);

            tripStatusDto.From = new PlaceDto
            {
                Longitude = from.Longitude,
                Latitude = from.Latitude
            };
            tripStatusDto.To = new PlaceDto
            {
                Longitude = to.Longitude,
                Latitude = to.Latitude
            };

            return Ok(tripStatusDto);
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
        public async Task<IActionResult> StartTrip([FromBody]LatLonDto location)
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

            #region StartNode
            var node = _mapper.Map<TripRouteNode>(location);

            node.UpdateTime = DateTime.UtcNow;

            node.TripId = trip.Id;

            await _tripsRepo.AddNode(node);

            trip.Distance = 0;

            trip.LastLat = location.Latitude;

            trip.LastLon = location.Longitude;

            trip.LastUpdateTime = node.UpdateTime;
            #endregion

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
        public async Task<IActionResult> FinishTripAsync([FromBody]LatLonDto finishTrip)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trip = _tripsRepo.GetTripByDriver(Guid.Parse(driverId), true);

            if (trip == null)
                return BadRequest();

            trip.FinishTime = DateTime.UtcNow;

            #region AddLastNode
            var node = _mapper.Map<TripRouteNode>(finishTrip);

            node.UpdateTime = DateTime.UtcNow;

            node.TripId = trip.Id;

            await _tripsRepo.AddNode(node);

            var delta = Helpers.Location.CalculateKilometersDistance(trip.LastLat, trip.LastLon, finishTrip.Latitude, finishTrip.Longitude);

            trip.Distance += delta;

            trip.LastLat = node.Latitude;

            trip.LastLon = node.Longitude;

            trip.LastUpdateTime = node.UpdateTime;

            #endregion

            var finishPlace = trip.Places.FirstOrDefault(p => p.IsTo == true);

            finishPlace.Latitude = finishTrip.Latitude;

            finishPlace.Longitude = finishTrip.Longitude;

            var tripHistory = _mapper.Map<TripHistory>(trip);

            var places = new List<FinishTripPlace>();

            foreach(var place in trip.Places)
            {
                places.Add(_mapper.Map<FinishTripPlace>(place));
            }
            tripHistory.Places = places;
            tripHistory.RouteNodes= trip.RouteNodes;
            //TODO : CalculatePrice
            tripHistory.Price = 0;

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
                Price = tripHistory.Price,
                Distance = tripHistory.Distance
            };//check if correctly maps from nullable
            return Ok(toReturn);
        }
    }
}
