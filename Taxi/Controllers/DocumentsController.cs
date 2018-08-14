using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Taxi.Entities;
using Taxi.Helpers;
using Taxi.Models;
using Taxi.Models.Drivers;
using Taxi.Services;

namespace Taxi.Controllers
{
    [Route("api/documents")]
    public class DocumentsController: Controller
    {
        private UserManager<AppUser> _userManager;
        private IUploadService _uploadService;
        private IUsersRepository _usersRepository;
        private IHostingEnvironment _hostingEnvironment;

        public DocumentsController(UserManager<AppUser> userManager,
            IUploadService uploadService,
            IUsersRepository usersRepository,
            IHostingEnvironment env)
        {
            _userManager = userManager;
            _uploadService = uploadService;
            _usersRepository = usersRepository;
            _hostingEnvironment = env;
        }

        [Authorize(Policy = "Driver")]
        [HttpGet("driverlicense")]
        public async Task<IActionResult> GetLicensePicture()
        {
            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var driver = _usersRepository.GetDriverById(Guid.Parse(driverId));

            if (driver?.DriverLicense == null)
                return NotFound();

            FileDto res = await _uploadService.GetObjectAsync(driver.DriverLicense.Id);

            if (res == null)
                return NotFound();

            res.Stream.Seek(0, SeekOrigin.Begin);
            return File(res.Stream, res.ContentType);
        }

        [Authorize(Policy = "Driver")]
        [HttpPut("driverlicense")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SetLicensePicture(List<IFormFile> files)
        {
            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var driver = _usersRepository.GetDriverById(Guid.Parse(driverId));

            long size = files.Sum(f => f.Length);

            var formFile = files[0];

            if (!formFile.IsImage())
            {
                return BadRequest();
            }

            if (driver.DriverLicense != null)
            {
                await _usersRepository.RemoveDriverLicense(driver.DriverLicense);
                //remove picture from data context
            }
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
                var imageId = Guid.NewGuid().ToString() + Path.GetExtension(filename);
                await _uploadService.PutObjectToStorage(imageId.ToString(), filename);//this is the method to upload saved file to S3
                await _usersRepository.AddDriverLicense(new DriverLicense()
                {
                    DriverId = driver.Id,
                    UpdateTime = DateTime.UtcNow,
                    Id = imageId
                });//dont save locally same names
                System.IO.File.Delete(filename);
                return Ok();
            }
            return BadRequest();
        }

        [Authorize(Policy = "Driver")]
        [HttpDelete("driverlicense")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> RemoveLicensePicture()
        {
            var driverId = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId)?.Value;

            var driver = _usersRepository.GetDriverById(Guid.Parse(driverId));

            if (driver.DriverLicense != null)
            {
                await _usersRepository.RemoveDriverLicense(driver.DriverLicense);
                //remove picture from data context
            }
            else return NotFound();

            return NoContent();
        }


    }
}
