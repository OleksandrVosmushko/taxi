﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Models.Trips;
using Taxi.Services;

namespace Taxi.Controllers
{
    [Route("api/tripshistory")]
    public class TripsHistoryController: Controller
    {
        private ITripsRepository _tripsRepository;

        public TripsHistoryController(ITripsRepository tripsRepository)
        {
            _tripsRepository = tripsRepository;
        }

        [HttpGet("driver")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> GetHistoryForDriver()
        {
            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trips = await _tripsRepository.GetTripHistoriesForDriver(Guid.Parse(driverId));

            var tripsToReturn = new List<TripHistoryDto>();
            
            foreach(var t in trips)
            {

                var from = t.Places.FirstOrDefault(p => p.IsFrom == true);
                var to = t.Places.FirstOrDefault(p => p.IsTo == true);

                tripsToReturn.Add(new TripHistoryDto()
                {
                    CustomerId = t.CustomerId,
                    DriverId = t.DriverId,

                    Id = t.Id,
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
                    FinishTime = t.FinishTime,
                    Price = t.Price
                });
            }

            return Ok(tripsToReturn);
        }

        [HttpGet("customer")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> GetHistoryForCustomer()
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;


            var trips = await _tripsRepository.GetTripHistoriesForCustomer(Guid.Parse(customerId));

            var tripsToReturn = new List<TripHistoryDto>();

            foreach (var t in trips)
            {
                var from = t.Places.FirstOrDefault(p => p.IsFrom == true);
                var to = t.Places.FirstOrDefault(p => p.IsTo == true);

                tripsToReturn.Add(new TripHistoryDto()
                {
                    CustomerId = t.CustomerId,
                    DriverId = t.DriverId,

                    Id = t.Id,
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
                    FinishTime = t.FinishTime,
                    Price = t.Price
                });
            }

            return Ok(tripsToReturn);
        }

    }
}
