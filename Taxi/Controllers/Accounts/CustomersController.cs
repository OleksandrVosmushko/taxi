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

        public CustomersController(UserManager<AppUser> userManager,
            IMapper mapper,
            IUsersRepository usersRepository,
            IEmailSender emailSender)
        {
            _mapper = mapper;
            _userManager = userManager;
            _usersRepository = usersRepository;
            _emailSender = emailSender;
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
    }
}
