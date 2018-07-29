using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Services;
using Taxi.Helpers;
using Taxi.Models;

namespace Taxi.Controllers
{
    [Route ("api/location")]
    public class LocationController: Controller
    {
        private IDriverLocationRepository _driverLocationRepository;

        public LocationController(IDriverLocationRepository driverLocationRepository)
        {
            _driverLocationRepository = driverLocationRepository;
        }

        //[Authorize(Policy = "Driver")]
        //[ProducesResponseType(204)]
        //[HttpPost("driver")]
        //public IActionResult AddDriverToMap(LatLonDto latLonDto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var driverid = User.Claims.Single(c => c.Type == Constants.Strings.JwtClaimIdentifiers.DriverId).Value;
        //    var res  = _driverLocationRepository.AddUser(Guid.Parse(driverid), latLonDto.Longitude, latLonDto.Latitude);

        //    if (res != true)
        //        return BadRequest();
        //    return NoContent();
        //}

        [Authorize(Policy = "Driver")]
        [ProducesResponseType(204)]
        [HttpPut("driver")]
        public IActionResult UpdateDriverLocation(LatLonDto latLonDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var driverid = User.Claims.Single(c => c.Type == Constants.Strings.JwtClaimIdentifiers.DriverId).Value;

            var res = _driverLocationRepository.UpdateUser(Guid.Parse(driverid), latLonDto.Longitude, latLonDto.Latitude, DateTime.Now);

            if (res != true)
                return BadRequest();
            return NoContent();
        }

        [Authorize(Policy = "Driver")]
        [ProducesResponseType(204)]
        [HttpDelete("driver")]
        public IActionResult RemoveDriverFromMap()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var driverid = User.Claims.Single(c => c.Type == Constants.Strings.JwtClaimIdentifiers.DriverId).Value;

            var res = _driverLocationRepository.RemoveUser(Guid.Parse(driverid));
            if (res != true)
                return BadRequest();
            
            return NoContent();
        }

        [Authorize(Policy = "Customer")]
        [ProducesResponseType(200)]
        [HttpGet]
        public IActionResult GetNearDrivers(LatLonDto latLonDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var ids = _driverLocationRepository.Search(latLonDto.Longitude, latLonDto.Latitude, 10000);

            var driverLocationToReturn = new List<DriverLocationDto>();

            foreach (var i in ids)
            {
                var s2cell = _driverLocationRepository.GetDriverLocation(i);

                if ((DateTime.Now - s2cell.UpdateTime).TotalSeconds < 60)
                {
                    var latlon = s2cell.CellId.ToLatLng();

                    driverLocationToReturn.Add(new DriverLocationDto()
                    {
                        DriverId = i,
                        Latitude = latlon.LatDegrees,
                        Longitude = latlon.LngDegrees
                    });
                }
                else
                {
                    _driverLocationRepository.RemoveUser(i);
                }
            }
            return Ok(driverLocationToReturn);
        }
    }
}
