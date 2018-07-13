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
    [Route ("api/accounts/drivers")]
    public class DriversController : Controller
    {
        private IMapper _mapper;
        private ApiUserManager _userManager;
        private IUsersRepository _usersRepository;

        public DriversController(ApiUserManager userManager, IMapper mapper, IUsersRepository usersRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _usersRepository = usersRepository;
        }

        
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
                    ModelState.AddModelError(nameof(DriverRegistrationDto.Email), "User name already taken");
                if (result.Errors.FirstOrDefault(o => o.Code == "DuplicatePhoneException") != null)
                    ModelState.AddModelError(nameof(DriverRegistrationDto.PhoneNumber), "Phone already used");
                return BadRequest(ModelState);
            }
            var driver = _mapper.Map<Driver>(model);
            driver.IdentityId = userIdentity.Id;

            await _usersRepository.AddDriver(driver);

            var driverDto = _mapper.Map<DriverDto>(model);

            _mapper.Map(userIdentity, driverDto);

            return CreatedAtRoute("GetDriver", new { id = userIdentity.Id }, driverDto);
        }

        [HttpGet(Name = "GetDriver")]
        [Route("{id}")]
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
