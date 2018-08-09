using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Services;

namespace Taxi.Controllers
{
    [Route("tripshistory")]
    public class TripsHistoryController: Controller
    {
        private ITripsRepository _tripsRepository;

        public TripsHistoryController(ITripsRepository tripsRepository)
        {
            _tripsRepository = tripsRepository;
        }

        [HttpGet("driver")]
        [Authorize(Policy = "Driver")]
        public IActionResult GetHistoryForDriver()
        {
            return Ok();
        }
    }
}
