using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Taxi.Entities;
using Taxi.Helpers;
using Taxi.Models;
using Taxi.Models.Trips;
using Taxi.Services;
using Location = Taxi.Helpers.Location;

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

            var from = trip.From;
            var to = trip.To;


            var tripToReturn = new UpdateResultTripDto()
            {
                CustomerId = trip.CustomerId,
                From = Taxi.Helpers.Location.PointToPlaceDto(from),
                To = Taxi.Helpers.Location.PointToPlaceDto(to),
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

            var customer = _usersRepository.GetCustomerById(Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value));

            if (customer.CurrentTrip != null)
                _tripsRepo.RemoveTrip(customer.Id);

            var tripEntity = new Trip()
            {
                CreationTime = DateTime.UtcNow,
                CustomerId = customer.Id,
                From = Helpers.Location.pointFromLatLng(tripCreationDto.From.Latitude, tripCreationDto.From.Longitude),
                To = Helpers.Location.pointFromLatLng(tripCreationDto.To.Latitude, tripCreationDto.To.Longitude)
            };

         // _tripsRepo.SetTrip(tripEntity);
            _tripsRepo.InsertTrip(tripEntity, tripCreationDto.From.Latitude,
                tripCreationDto.From.Longitude,
                tripCreationDto.To.Latitude, tripCreationDto.To.Longitude);
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
        public async Task<IActionResult> UpdateTripStartLocation([FromBody]LatLonDto location)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var customerId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value);

            var trip = _tripsRepo.GetTrip(customerId);

            if (trip == null)
                return NotFound();

            var res = await _tripsRepo.UpdateTrip(trip, Mapper.Map<PlaceDto>(location));

            if (res == false)
                return BadRequest();

            return NoContent();
        }
        
        [HttpGet(Name = "GetNearTrips")]
        [Authorize(Policy = "Driver")]
        [ProducesResponseType(200)]
        public IActionResult GetNearTrips(GetTripsResourceParameters resourceParameters)
        {
            var toReturn = _tripsRepo.GetNearTrips(resourceParameters.Longitude, resourceParameters.Latitude, resourceParameters);

            var prevLink = toReturn.HasPrevious
                ? CreateTripsResourceUri(resourceParameters, ResourceUriType.PrevoiusPage, nameof(GetNearTrips)) : null;

            var nextLink = toReturn.HasNext
                ? CreateTripsResourceUri(resourceParameters, ResourceUriType.NextPage, nameof(GetNearTrips)) : null;

            Response.Headers.Add("X-Pagination", Helpers.PaginationMetadata.GeneratePaginationMetadata(toReturn, resourceParameters, prevLink, nextLink));

            return Ok(toReturn.ToList());
        }

        private string CreateTripsResourceUri(GetTripsResourceParameters resourceParameters, ResourceUriType type, string getMethodName)
        {
            switch (type)
            {
                case ResourceUriType.PrevoiusPage:
                    return _urlHelper.Link(getMethodName,
                        new
                        {
                            pageNumber = resourceParameters.PageNumber - 1,
                            pageSize = resourceParameters.PageSize,
                            longitude = resourceParameters.Longitude,
                            latitude = resourceParameters.Latitude
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link(getMethodName,
                        new
                        {
                            pageNumber = resourceParameters.PageNumber + 1,
                            pageSize = resourceParameters.PageSize,
                            longitude = resourceParameters.Longitude,
                            latitude = resourceParameters.Latitude
                        });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link(getMethodName,
                        new
                        {
                            pageNumber = resourceParameters.PageNumber,
                            pageSize = resourceParameters.PageSize,
                            longitude = resourceParameters.Longitude,
                            latitude = resourceParameters.Latitude
                        });
            }
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

            var from = trip.From;
            var to = trip.To;

            tripStatusDto.From = Taxi.Helpers.Location.PointToPlaceDto(from);

            tripStatusDto.To = Taxi.Helpers.Location.PointToPlaceDto(to);

            return Ok(tripStatusDto);
        }

        [Authorize(Policy = "Driver")]
        [HttpPost("taketrip")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> AddDriverToTrip(Guid customerId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var trip = _tripsRepo.GetTrip(customerId);
           
            if (trip == null || driverId == null || trip.DriverId != null) 
                return BadRequest();

            trip.DriverId = Guid.Parse(driverId);

            trip.DriverTakeTripTime = DateTime.UtcNow;

            var res = await _tripsRepo.UpdateTrip(trip);

            if (res != true)
                return BadRequest();
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

            var res = await _tripsRepo.UpdateTrip(trip, Mapper.Map<PlaceDto>(location));

            if (res != true)
                return BadRequest();
            
            var from = trip.From;
            var to = trip.To;
            var customer = _usersRepository.GetCustomerById(trip.CustomerId);
            var toReturn = new TripDto()
            {
                CustomerId = trip.CustomerId,
                From = Helpers.Location.PointToPlaceDto(from),
                To = Helpers.Location.PointToPlaceDto(to),
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

            var res = await _tripsRepo.UpdateTrip(trip, null, Mapper.Map<PlaceDto>(finishTrip));

            if (res == false)
                return BadRequest();
            #endregion
            
            var tripHistory = _mapper.Map<TripHistory>(trip);
 
            foreach(var rnode in trip.RouteNodes)
            {
                tripHistory.TripHistoryRouteNodes.Add(Mapper.Map<TripHistoryRouteNode>(rnode));
            }

            //TODO : CalculatePrice
            tripHistory.Price = 0;

            await _tripsRepo.AddTripHistory(tripHistory);

            _tripsRepo.RemoveTrip(trip.CustomerId);

            var from = tripHistory.From;
            var to = tripHistory.To;

            var toReturn = new TripHistoryDto()
            {
                CustomerId = tripHistory.CustomerId,
                DriverId = tripHistory.DriverId,

                Id = tripHistory.Id,
                From = Helpers.Location.PointToPlaceDto(from),
                To = Helpers.Location.PointToPlaceDto(to),
                FinishTime = tripHistory.FinishTime,
                Price = tripHistory.Price,
                Distance = tripHistory.Distance
            };//check if correctly maps from nullable
            return Ok(toReturn);
        }
    }
}
