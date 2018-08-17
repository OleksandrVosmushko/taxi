using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Taxi.Controllers
{
    [Route("api/admins")]
    public class AdminController: Controller
    {
        public AdminController()
        {
            
        }

        [Authorize(Policy = "Root")]
        [HttpGet]
        public IActionResult GetAdmins()
        {
            return Ok();
        }

    }
}
