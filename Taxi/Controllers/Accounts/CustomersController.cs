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
    [Route("api/accounts/customers")]
    public class CustomersController : Controller
    {

        private IMapper _mapper;
        private UserManager<AppUser> _userManager;
        private IUsersRepository _usersRepository;

        public CustomersController(UserManager<AppUser> userManager, IMapper mapper, IUsersRepository usersRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _usersRepository = usersRepository;
        }

        [HttpGet(Name = "GetCustomer")]
        [Route("{id}")]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            var customerIdentity = await _userManager.FindByIdAsync(id.ToString());

            if (customerIdentity == null)
            {
                return NotFound();
            }
            var customer = _usersRepository.GetCustomerByIdentityId(id.ToString());

            if (customer == null)
            {
                return NotFound();
            }

            var customerDto = _mapper.Map<Customer, CustomerDto>(customer);

            _mapper.Map(customerIdentity, customerDto);

            return Ok(customerDto);
        }
        
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

            _mapper.Map(userIdentity, customerDto);

            return CreatedAtRoute("GetCustomer", new { id = userIdentity.Id }, customerDto);
        }
    }
}
