﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Taxi.Helpers;
using Taxi.Models;
using Taxi.Models.Trips;
using Taxi.Services;

namespace Taxi.Controllers
{
    [Route("api/tripshistory")]
    public class TripsHistoryController: Controller
    {
        private ITripsRepository _tripsRepository;
        private IUrlHelper _urlHelper;

        public TripsHistoryController(ITripsRepository tripsRepository,
            IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
            _tripsRepository = tripsRepository;
        }
        private string CreateResourceUri(PaginationParameters resourceParameters, ResourceUriType type, string getMethodName)
        {
            switch (type)
            {
                case ResourceUriType.PrevoiusPage:
                    return _urlHelper.Link(getMethodName,
                        new
                        {
                            pageNumber = resourceParameters.PageNumber - 1,
                            pageSize = resourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link(getMethodName,
                        new
                        {
                            pageNumber = resourceParameters.PageNumber + 1,
                            pageSize = resourceParameters.PageSize
                        });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link(getMethodName,
                        new
                        {
                            pageNumber = resourceParameters.PageNumber,
                            pageSize = resourceParameters.PageSize
                        });
            }
        }
      

        [HttpGet("driver", Name = "GetDriverHistory")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> GetDriverHistory(TripHistoryResourceParameters resourceParameters)
        {
            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trips =  _tripsRepository.GetTripHistoriesForDriver(Guid.Parse(driverId), resourceParameters);


            var prevLink = trips.HasPrevious
                ? CreateResourceUri(resourceParameters, ResourceUriType.PrevoiusPage, nameof(GetDriverHistory)) : null;

            var nextLink = trips.HasNext
                ? CreateResourceUri(resourceParameters, ResourceUriType.NextPage, nameof(GetDriverHistory)) : null;

            Response.Headers.Add("X-Pagination", Helpers.PaginationMetadata.GeneratePaginationMetadata(trips, resourceParameters, prevLink, nextLink));

            var tripsToReturn = new List<TripHistoryDto>();
            
            foreach(var t in trips)
            {

                var from = t.From;
                var to = t.To;

                tripsToReturn.Add(new TripHistoryDto()
                {
                    CustomerId = t.CustomerId,
                    DriverId = t.DriverId,

                    Id = t.Id,
                    From = Helpers.Location.PointToPlaceDto(from),
                    To = Helpers.Location.PointToPlaceDto(to),
                    FinishTime = t.FinishTime,
                    Price = t.Price,
                    Distance = t.Distance
                });
            }

            return Ok(tripsToReturn);
        }

        [HttpGet("driver/triproute/{tripHistoryId}")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> GetDriverTripRoute(Guid tripHistoryId)
        {
            var trip = await _tripsRepository.GetTripHistory(tripHistoryId);

            if (trip == null)
                return NotFound();

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            if (trip.DriverId != Guid.Parse(driverId))
            {
                return StatusCode(403);
            }

            var tripRoute = await _tripsRepository.GetTripRouteNodes(tripHistoryId);

            var routesDto = new List<RouteNodeDto>();
            foreach (var r in tripRoute)
            {
                routesDto.Add(Mapper.Map<RouteNodeDto>(r));
            }

            return Ok(routesDto);
        }
        [HttpGet("customer/triproute/{tripHistoryId}")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> GetCustomerTripRoute(Guid tripHistoryId)
        {
            var trip = await _tripsRepository.GetTripHistory(tripHistoryId);

            if (trip == null)
                return NotFound();

            var customerId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;

            if (trip.CustomerId != Guid.Parse(customerId))
            {
                return StatusCode(403);
            }

            var tripRoute = await _tripsRepository.GetTripRouteNodes(tripHistoryId);

            var routesDto = new List<RouteNodeDto>();
            foreach (var r in tripRoute)
            {
                routesDto.Add(Mapper.Map<RouteNodeDto>(r));
            }
            
            return Ok(routesDto);
        }

        [HttpGet("customer",Name = "GetCustomerHistory")]
        [Authorize(Policy = "Customer")]
        public async Task<IActionResult> GetCustomerHistory(TripHistoryResourceParameters resourceParameters)
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;


            var trips =  _tripsRepository.GetTripHistoriesForCustomer(Guid.Parse(customerId),resourceParameters);

            var prevLink = trips.HasPrevious
                ? CreateResourceUri(resourceParameters, ResourceUriType.PrevoiusPage, nameof(GetCustomerHistory)):null;

            var nextLink = trips.HasNext
                ?CreateResourceUri(resourceParameters, ResourceUriType.NextPage, nameof(GetCustomerHistory)) : null;

            
            Response.Headers.Add("X-Pagination", Helpers.PaginationMetadata.GeneratePaginationMetadata(trips, resourceParameters, prevLink, nextLink));

            var tripsToReturn = new List<TripHistoryDto>();

            foreach (var t in trips)
            {
                var from = t.From;
                var to = t.To;
                
                tripsToReturn.Add(new TripHistoryDto()
                {
                    CustomerId = t.CustomerId,
                    DriverId = t.DriverId,

                    Id = t.Id,
                    From = Helpers.Location.PointToPlaceDto(from),
                    To = Helpers.Location.PointToPlaceDto(to),
                    FinishTime = t.FinishTime,
                    Price = t.Price,
                    Distance = t.Distance
                });
            }

            return Ok(tripsToReturn);
        }

    }
}
