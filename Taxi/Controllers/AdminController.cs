using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Taxi.Entities;
using Taxi.Helpers;
using Taxi.Models;
using Taxi.Models.Admins;
using Taxi.Models.Drivers;
using Taxi.Models.Trips;
using Taxi.Services;

namespace Taxi.Controllers
{
    [Route("api/admins")]
    public class AdminController: Controller
    {
        private IUsersRepository _usersRepository;
        private IResourceUriHelper _resourceUriHelper;
        private UserManager<AppUser> _userManager;
        private IEmailSender _emailSender;
        private IUploadService _uploadService;
        private ITripsRepository _tripsRepository;

        public AdminController(IUsersRepository usersRepository,
            IResourceUriHelper resourceUriHelper,
            UserManager<AppUser> userManager,
            IEmailSender emailSender,
            IUploadService uploadService,
            ITripsRepository tripsRepository)
        {
            _usersRepository = usersRepository;
            _resourceUriHelper = resourceUriHelper;
            _userManager = userManager;
            _emailSender = emailSender;
            _uploadService = uploadService;
            _tripsRepository = tripsRepository;
        }

        [HttpGet("{id}", Name = "GetAdmin")]
        public IActionResult GetAdmin(Guid id)
        {
            var admin = _usersRepository.GetAdminById(id);

            if (admin?.Identity == null )
            {
                return NotFound();
            }

            var adminDto = Mapper.Map<AdminDto>(admin.Identity);

            Mapper.Map(admin, adminDto);

            adminDto.ProfilePictureId = admin.Identity.ProfilePicture?.Id;

            return Ok(adminDto);
        }
        
        [Authorize(Policy = "Admin")]
        [HttpGet("getusers",Name = "GetUsers")]
        public async Task<IActionResult> GetUsers(UserResourceParameters resourceParameters)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var users = await _usersRepository.GetUsers(resourceParameters);

            var prevLink = users.HasPrevious
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.PrevoiusPage, nameof(GetUsers)) : null;

            var nextLink = users.HasNext
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.NextPage, nameof(GetUsers)) : null;

            Response.Headers.Add("X-Pagination", Helpers.PaginationMetadata.GeneratePaginationMetadata(users, resourceParameters, prevLink, nextLink));

            List<UserDto> userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var userDto = Mapper.Map<UserDto>(user);

                var claims = await _userManager.GetClaimsAsync(user);

                foreach (var c in claims)
                {
                    if (c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.Rol)
                        userDto.Roles.Add(c.Value);
                    if (c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId ||
                        c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId ||
                        c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.AdminId)
                    {
                        userDto.Ids[c.Type] = c.Value;
                    }
                }
                userDtos.Add(userDto);
            }
            return Ok(userDtos);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("getuser/{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = Mapper.Map<UserDto>(user);

            var claims = await _userManager.GetClaimsAsync(user);

            foreach (var c in claims)
            {
                if (c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.Rol)
                    userDto.Roles.Add(c.Value);
                if (c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId ||
                    c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.DriverId ||
                    c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.AdminId)
                {
                    userDto.Ids[c.Type] = c.Value;
                }
            }

            return Ok(userDto);
        }



        [Authorize(Policy = "Admin")]
        [HttpPost("response")]
        public async Task<IActionResult> WriteResponse([FromBody]AdminResponseDto responseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var adminid = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.AdminId)?.Value;

            var responce = Mapper.Map<AdminResponse>(responseDto);

            responce.CreationTime = DateTime.UtcNow;

            responce.AdminId = Guid.Parse( adminid);

            await _usersRepository.AddAdminResponse(responce);

            return Ok();
        }



        [Authorize(Policy = "Admin")]
        [HttpGet("refundRequests")]
        public IActionResult GetRefundRequests(RefundResourceParameters resourceParameters)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refundsRequests = _usersRepository.GetRefundRequests(resourceParameters);

            var prevLink = refundsRequests.HasPrevious
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.PrevoiusPage, nameof(GetUsers)) : null;

            var nextLink = refundsRequests.HasNext
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.NextPage, nameof(GetUsers)) : null;

            Response.Headers.Add("X-Pagination", Helpers.PaginationMetadata.GeneratePaginationMetadata(refundsRequests, resourceParameters, prevLink, nextLink));

            var refundDtos = new List<RefundRequestDto>();

            foreach (var r in refundsRequests)
            {
                refundDtos.Add(Mapper.Map<RefundRequestDto>(r));
            }

            return Ok(refundDtos);
        }

        //[Authorize(Policy = "Admin")]
        //[HttpPost("refundRequests/solve/{refundRequestId}")]
        //public IActionResult SolveRefund(Guid refundRequestId, [FromBody] RefundSolutionDto solution)
        //{
            
        //}


        [Authorize(Policy = "Admin")]
        [HttpGet("driverlicenses/{driverId}")]
        public IActionResult GetDriverLicense(Guid driverId)
        {
            
            var driver = _usersRepository.GetDriverById(driverId);

            if (driver?.DriverLicense == null)
                return NotFound();

            var licenseToReturn = Mapper.Map<DriverLicenseDto>(driver.DriverLicense);

            return Ok(licenseToReturn);
        }
        
        [Authorize(Policy = "Admin")]
        [HttpGet("driverlicenses/{driverId}/image")]
        public async Task<IActionResult> GetLicensePicture(Guid driverId)
        {
            var driver = _usersRepository.GetDriverById(driverId);

            if (driver?.DriverLicense == null)
                return NotFound();

            FileDto res = await _uploadService.GetObjectAsync(driver.DriverLicense.ImageId);

            if (res == null)
                return NotFound();

            res.Stream.Seek(0, SeekOrigin.Begin);
            return File(res.Stream, res.ContentType);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet("driverlicenses")]
        public IActionResult GetDriverLicenses(DriverLicenseResourceParameters resourceParameters)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var licenses = _usersRepository.GetDriverLicenses(resourceParameters);

            var prevLink = licenses.HasPrevious
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.PrevoiusPage, nameof(GetUsers)) : null;

            var nextLink = licenses.HasNext
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.NextPage, nameof(GetUsers)) : null;

            Response.Headers.Add("X-Pagination", Helpers.PaginationMetadata.GeneratePaginationMetadata(licenses, resourceParameters, prevLink, nextLink));

            var licensesDtos = new List<DriverLicenseDto>();

            foreach (var l in licenses)
            {
                licensesDtos.Add(Mapper.Map<DriverLicenseDto>(l));
            }

            return Ok(licensesDtos);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("driverlicenses/{driverId}/approve")]
        public async Task<IActionResult> ApproveLicense(Guid driverId)
        {
            var driver = _usersRepository.GetDriverById(driverId);

            if (driver?.DriverLicense == null)
                return NotFound();

            driver.DriverLicense.IsApproved = true;

            await _usersRepository.UpdateDriverLicense(driver.DriverLicense);
            
            return Ok();
        }
        
        [Authorize(Policy = "Admin")]
        [HttpDelete("removeuser/{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> RemoveUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var claims = await _userManager.GetClaimsAsync(user);

            foreach (var c in claims)
            {
                if (c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.Rol &&
                    c.Value == Helpers.Constants.Strings.JwtClaims.AdminAccess)
                    return StatusCode(403);
            }

            await _usersRepository.RemoveUser(user);

            return NoContent();
        }

        [Authorize(Policy = "Root")]
        [HttpGet(Name = "GetAdmins")]
        public IActionResult GetAdmins(AdminResourceParameters resourceParameters)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var admins =_usersRepository.GetAdmins(resourceParameters);

            var prevLink = admins.HasPrevious
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.PrevoiusPage, nameof(GetAdmins)) : null;

            var nextLink = admins.HasNext
                ? _resourceUriHelper.CreateResourceUri(resourceParameters, ResourceUriType.NextPage, nameof(GetAdmins)) : null;

            Response.Headers.Add("X-Pagination", Helpers.PaginationMetadata.GeneratePaginationMetadata(admins, resourceParameters, prevLink, nextLink));
            
            List<AdminDto> toReturn = new List<AdminDto>();

            foreach (var a in admins)
            {
                toReturn.Add(new AdminDto()
                {
                    Id = a.Id,
                    FirstName = a.Identity.FirstName,
                    LastName = a.Identity.LastName,
                    IsApproved = a.IsApproved,
                    ProfilePictureId = a.Identity?.ProfilePicture?.Id
                });
            }
            return Ok(toReturn);
        }
       
        [Authorize(Policy = "Root")]
        [HttpPost("root/userToAdmin/{id}")]
        public async Task<IActionResult> UserToAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var admin = new Admin(){IdentityId = id, IsApproved = true};

            await _usersRepository.AddAdmin(admin);

            return Ok();
        }

        [Authorize(Policy = "Root")]
        [HttpPost("root/approve/{adminId}")]
        public async Task<IActionResult> ApproveAdmin(Guid adminId)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var admin = _usersRepository.GetAdminById(adminId);

            if (admin == null)
                return NotFound();
            
            await _usersRepository.ApproveAdmin(admin);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAdmin([FromBody]AdminRegistrationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userIdentity = Mapper.Map<AppUser>(model);

            var result = await _userManager.CreateAsync(userIdentity, model.Password);

            if (!result.Succeeded)
            {
                if (result.Errors.FirstOrDefault(o => o.Code == "DuplicateUserName") != null)
                    ModelState.AddModelError(nameof(CustomerRegistrationDto), "User name already taken");
                return BadRequest(ModelState);
            }
            var admin = Mapper.Map<Admin>(model);

            admin.IdentityId = userIdentity.Id;

            await _usersRepository.AddAdmin(admin);

            var customerFromAdmin = Mapper.Map<Customer>(admin);

            await _usersRepository.AddCustomer(customerFromAdmin);

            var adminDto = Mapper.Map<AdminDto>(model);

            adminDto.Id = admin.Id;

            if (!userIdentity.EmailConfirmed)
            {
                var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(userIdentity);
                var emailConfirmUrl = Url.RouteUrl("ConfirmEmail", new { uid = userIdentity.Id, token = confirmToken }, this.Request.Scheme);
                try
                {
                    await _emailSender.SendEmailAsync(userIdentity.Email, "Confirm your account",
                        $"Please confirm your account by this ref <a href=\"{emailConfirmUrl}\">link</a>");
                }
                catch
                {
                    ModelState.AddModelError("email", "Failed to send confirmation letter");
                    return BadRequest(ModelState);
                }
            }
            return CreatedAtRoute("GetAdmin", new { id = admin.Id }, adminDto);
        }
    }
}
