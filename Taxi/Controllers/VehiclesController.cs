using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models.Drivers;
using Taxi.Services;

namespace Taxi.Controllers
{
    [Route("api/vehicles")]
    public class VehiclesController: Controller
    {
        private IMapper _mapper;
        private UserManager<AppUser> _userManager;
        private IUsersRepository _usersRepository;
        private IUploadService _uploadService;
        private IHostingEnvironment _hostingEnvironment;

        public VehiclesController(UserManager<AppUser> userManager, IMapper mapper, IUsersRepository usersRepository, IUploadService uploadService, IHostingEnvironment env )
        {
            _mapper = mapper;
            _userManager = userManager;
            _usersRepository = usersRepository;
            _uploadService = uploadService;
            _hostingEnvironment = env;
        }

        [HttpPost()]
        [Authorize(Policy = "Driver")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> AddVehicleToDriver(AddVehicleDto vehicle)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var vehicleEntity = _mapper.Map<Vehicle>(vehicle);

            var res = await _usersRepository.AddVehicleToDriver(vehicleEntity);

            if (res != true)
            {
                return BadRequest();
            }
            var vehicleToReturn = _mapper.Map<VehicleToReturnDto>(vehicleEntity);
            return CreatedAtRoute("GetVehicle", new { id = vehicleEntity.Id }, vehicleToReturn);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Driver")]
        public async Task<IActionResult> RemoveVehicle(Guid id)
        {
            var vehicle = await _usersRepository.GetVehicle(id);

            if (vehicle == null)
            {
                return NotFound();
            }

            await _usersRepository.RemoveVehicle(vehicle);

            return NoContent();
        }
        
        [HttpGet("{id}", Name = "GetVehicle")]
        [Authorize]
        public async Task<IActionResult> GetVehicle(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var vehicle = await _usersRepository.GetVehicle(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            var vehicleToReturn = _mapper.Map<VehicleToReturnDto>(vehicle);
            return Ok(vehicleToReturn);
        }
        [HttpPost("up")]
        public async Task<IActionResult> Upload()
        {
            await _uploadService.PutObjectToStorage(Guid.NewGuid().ToString(), "2");
            return Ok();
        }
        [HttpPost("images")]
        [Authorize(Policy = "Driver")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            //var files = new List<IFormFile>();

            //files.Add(file);

            long size = files.Sum(f => f.Length);

            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var driver = _usersRepository.GetDriverById(Guid.Parse(driverId));
           
            //if (driver.Vehicle == null)
            //{
            //    ModelState.AddModelError(nameof(driver.Vehicle), "Driver has no vehicle to add images");

            //    return BadRequest(ModelState);
            //}
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filename = ContentDispositionHeaderValue
                            .Parse(formFile.ContentDisposition)
                            .FileName
                            .TrimStart().ToString();
                    filename = _hostingEnvironment.WebRootPath + $@"\uploads" + $@"\{formFile.FileName}";
                    size += formFile.Length;
                    using (var fs = System.IO.File.Create(filename))
                    {
                        await formFile.CopyToAsync(fs);
                        fs.Flush();
                    }//these code snippets saves the uploaded files to the project directory

                    await _uploadService.PutObjectToStorage(Guid.NewGuid().ToString(), filename);//this is the method to upload saved file to S3

                //    System.IO.File.Delete(filename);
                }
            }
            return Ok();
        }

        [HttpGet("images")]
 
        public async Task<IActionResult> Get()
        {
            FileDto res = await _uploadService.GetObjectAsync("8c6ac8ba-dd3e-403d-a75b-4d4ad90eba1f");

        //    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(res.Stream));

         //   return Ok();
              return File(Encoding.UTF8.GetBytes(res.Stream), res.ContentType);

        }


    }
}
