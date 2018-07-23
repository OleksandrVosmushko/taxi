using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models;
using Taxi.Models.Customers;
using Taxi.Models.Trips;
using Taxi.Services;

namespace Taxi.Controllers.Accounts
{
    [Route("api/accounts/customers")]
    public class CustomersController : Controller
    {

        private IMapper _mapper;
        private UserManager<AppUser> _userManager;
        private IUsersRepository _usersRepository;
        private IEmailSender _emailSender;
        private ITripsRepository _tripsCashe;

        public CustomersController(UserManager<AppUser> userManager,
            IMapper mapper,
            IUsersRepository usersRepository,
            IEmailSender emailSender,
            ITripsRepository tripsCashe)
        {
            _mapper = mapper;
            _userManager = userManager;
            _usersRepository = usersRepository;
            _emailSender = emailSender;
            _tripsCashe = tripsCashe;
        }

        [Produces(contentType: "application/json")]
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            var customer = _usersRepository.GetCustomerById(id);

            if (customer == null)
            {
                return NotFound();
            }

            var customerIdentity = await _userManager.FindByIdAsync(customer.IdentityId);

            if (customerIdentity == null)
            {
                return NotFound();
            }
         
            var customerDto = _mapper.Map<CustomerDto>(customerIdentity);

            _mapper.Map(customer, customerDto);

            return Ok(customerDto);
        }
        [ProducesResponseType(201)]
        [Produces(contentType: "application/json")]
        [HttpPost]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegistrationDto model)
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

            var customer = _mapper.Map<Customer>(model);
            customer.IdentityId = userIdentity.Id;

            await _usersRepository.AddCustomer(customer);

            var customerDto = _mapper.Map<CustomerDto>(model);

            customerDto.Id = customer.Id;

            if (!userIdentity.EmailConfirmed)
            {
                var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(userIdentity);
                var emailConfirmUrl = Url.RouteUrl("ConfirmEmail", new {uid = userIdentity.Id, token = confirmToken},
                    this.Request.Scheme);
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

            return CreatedAtRoute("GetCustomer", new { id = customer.Id }, customerDto);
        }

        [HttpPut("{id}")]
        [Authorize (Policy = "Customer")]
        [ProducesResponseType(204)]
        public async Task <IActionResult> UpdateCustomer(Guid id, CustomerUpdateDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var customer = _usersRepository.GetCustomerById(id);

            if (customer == null)
            {
                return NotFound();
            }

            Mapper.Map(customerDto, customer.Identity);
            Mapper.Map(customerDto, customer);


            if (customerDto.CurrentPassword != null && customerDto.NewPassword != null)
            {
                var result = await _userManager.ChangePasswordAsync(customer.Identity, customerDto.CurrentPassword, customerDto.NewPassword);
                if (!result.Succeeded)
                {
                    return BadRequest();
                }
            }

            await _usersRepository.UpdateCustomer(customer);

            return NoContent();
        }

        [Authorize (Policy = "Customer")]
        [HttpPost("trip")] 
        public IActionResult CreateTrip(TripCreationDto tripCreationDto)
        {
            var tripEntity = Mapper.Map<Trip>(tripCreationDto);

            tripEntity.CreationTime = DateTime.UtcNow;

            tripEntity.Id = Guid.NewGuid();

            _tripsCashe.AddTrip(tripEntity);

            return Ok();
        }

        [Authorize(Policy = "Customer")]
        [HttpDelete("trip")]
        public IActionResult DeleteTripForCustomer()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.CustomerId)?.Value;
            
            _tripsCashe.RemoveTrip(Guid.Parse(id));

            return NoContent();
        }

        [Authorize(Policy = "Customer")]
        [HttpPut("trip")]
        public IActionResult UpdateTrip(TripCreationDto tripCreationDto)
        {
            var TripToUpdate = _tripsCashe.GetTrip(tripCreationDto.CustomerId);


            return Ok();
        }
    }
}
