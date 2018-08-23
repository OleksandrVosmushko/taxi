using System;
using System.Collections.Generic;
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

        public AdminController(IUsersRepository usersRepository,
            IResourceUriHelper resourceUriHelper,
            UserManager<AppUser> userManager,
            IEmailSender emailSender)
        {
            _usersRepository = usersRepository;
            _resourceUriHelper = resourceUriHelper;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet("id", Name = "GetAdmin")]
        public IActionResult GetAdmin(Guid id)
        {
            var admin = _usersRepository.GetAdminById(id);

            if (admin?.Identity == null )
            {
                return NotFound();
            }

            var adminDto = Mapper.Map<AdminDto>(admin.Identity);

            Mapper.Map(admin, adminDto);

            adminDto.ProfilePictureId = admin.Identity.ProfilePicture.Id;

            return Ok(adminDto);
        }

        [Authorize(Policy = "Root")]
        [HttpGet(Name = "GetAdmins")]
        public IActionResult GetAdmins(AdminResourceParameters resourceParameters)
        {
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
                    ProfilePictureId = a.Identity.ProfilePicture.Id
                });
            }
            return Ok(admins);
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
