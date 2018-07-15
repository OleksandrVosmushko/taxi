using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models;
using Taxi.Services;

namespace Taxi.Controllers.Accounts
{
    [Route("api/accounts/drivers")]
    public class DriversController : Controller
    {
        private IMapper _mapper;
        private UserManager<AppUser> _userManager;
        private IUsersRepository _usersRepository;
        private IEmailSender _emailSender;

        public DriversController(UserManager<AppUser> userManager, IMapper mapper, IUsersRepository usersRepository, IEmailSender emailSender)
        {
            _mapper = mapper;
            _userManager = userManager;
            _usersRepository = usersRepository;
            _emailSender = emailSender;
        }

        [ProducesResponseType(201)]
        [HttpPost]
        public async Task<IActionResult> RegisterDriver([FromBody] DriverRegistrationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userIdentity = _mapper.Map<AppUser>(model);

            var result = await _userManager.CreateAsync(userIdentity, model.Password);

            if (!result.Succeeded)
            {
                if (result.Errors.FirstOrDefault(o => o.Code == "DuplicateUserName") != null)
                    ModelState.AddModelError(nameof(CustomerRegistrationDto), "User name already taken");
                return BadRequest(ModelState);
            }
            var driver = _mapper.Map<Driver>(model);
            driver.IdentityId = userIdentity.Id;

            await _usersRepository.AddDriver(driver);

            var driverDto = _mapper.Map<DriverDto>(model);

            _mapper.Map(userIdentity, driverDto);

            if (!userIdentity.EmailConfirmed)
            {
                var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(userIdentity);
                var emailConfirmUrl = Url.RouteUrl("ConfirmEmail", new { uid = userIdentity.Id, token = confirmToken }, this.Request.Scheme);
                await _emailSender.SendEmailAsync(userIdentity.Email, "Confirm your account",
                    $"Please confirm your account by this ref <a href=\"{emailConfirmUrl}\">link</a>");
            }

            return CreatedAtRoute("GetDriver", new { id = userIdentity.Id }, driverDto);
        }

        [HttpGet("{id}",Name = "GetDriver")]
        public async Task<IActionResult> GetDriver(Guid id)
        {
            var driverIdentity = await _userManager.FindByIdAsync(id.ToString());

            if (driverIdentity == null)
            {
                return NotFound();
            }
            var driver = _usersRepository.GetDriverByIdentityId(id.ToString());

            if (driver == null)
            {
                return NotFound();
            }

            var driverDto = _mapper.Map<Driver, DriverDto>(driver);

            _mapper.Map(driverIdentity, driverDto);

            return Ok(driverDto);
        }


    }
}
