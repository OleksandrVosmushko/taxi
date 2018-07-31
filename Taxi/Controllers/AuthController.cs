using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Auth;
using Taxi.Entities;
using Taxi.Models;
using System.Security.Claims;

using Newtonsoft.Json;
using Taxi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Taxi.Services;
using Taxi.Models.Customers;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace Taxi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private UserManager<AppUser> _userManager;
        private IJwtFactory _jwtFactory;
        private JwtIssuerOptions _jwtOptions;
        private IEmailSender _emailSender;
        private IUsersRepository _userRepository;
        private IMapper _mapper;
        private IHttpContextAccessor _httpContextAccessor;

        public AuthController(UserManager<AppUser> userManager, 
            IJwtFactory jwtFactory,
            IOptions<JwtIssuerOptions> jwtOptions, 
            IEmailSender emailSender, 
            IUsersRepository usersRepository,
            IMapper mapper,
            IHttpContextAccessor  httpContextAccessor)
        {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _emailSender = emailSender;
            _userRepository = usersRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }



        [HttpGet()]
        [Authorize(Policy = "Driver")]
        public IActionResult GetAuthorizedOnly()
        {
            return Ok(1);
        }
        [Produces(contentType: "application/json")]
        [HttpPost("customer")]
        public async Task<IActionResult> LoginCustomer([FromBody]CreditionalsDto credentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
            if (identity == null)
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
            }
            // Ensure the email is confirmed.
            var customer = _userRepository.GetCustomerByIdentityId(identity.Claims.Single(c => c.Type == Constants.Strings.JwtClaimIdentifiers.Id).Value);
            
            if (customer == null)
            {
                return NotFound();
            }
            var ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];
            var jwt = await Tokens.GenerateJwt(identity, _jwtFactory, credentials.UserName, _jwtOptions, customer.Id, ip,userAgent);

            return Ok(JsonConvert.DeserializeObject(jwt)); ;
        }


        [Authorize(Policy = "Customer")]
        [HttpPost("customerdriver")]
        [ProducesResponseType(201)]
        public async Task<IActionResult> MakeCustomerDriver([FromBody]CustomerDriverUpgradeDto customerDriverUpgradeDto )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var uid = User.Claims.Single(c => c.Type == Constants.Strings.JwtClaimIdentifiers.Id).Value;

            var customer = _userRepository.GetCustomerByIdentityId(uid);
            if (customer == null)
            {
                return NotFound();
            }
            var testDriver = _userRepository.GetDriverByIdentityId(uid);

            if (testDriver != null)
            {
                return BadRequest();
            }

            var driver = _mapper.Map<Driver>(customer);

            _mapper.Map(customerDriverUpgradeDto, driver);

            await _userRepository.AddDriver(driver);

            var user = await _userManager.FindByIdAsync(uid);

            var driverDto = _mapper.Map<DriverDto>(user);

            _mapper.Map(driver, driverDto);

            return CreatedAtRoute("GetDriver", new { id = driver.Id }, driverDto);
        }


        [Produces(contentType: "application/json")]
        [HttpPost("driver")]
        public async Task<IActionResult> LoginDriver([FromBody]CreditionalsDto credentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
            if (identity == null)
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
            }
            // Ensure the email is confirmed.
            var driver = _userRepository.GetDriverByIdentityId(identity.Claims.Single(c => c.Type == Constants.Strings.JwtClaimIdentifiers.Id).Value);
            
            if (driver == null)
            {
                return NotFound();
            }
            var ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];

            var jwt = await Tokens.GenerateJwt(identity, _jwtFactory, credentials.UserName, _jwtOptions, driver.Id, ip, userAgent);

            return Ok(JsonConvert.DeserializeObject(jwt)); 
        }


        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            var looked = await _userManager.IsLockedOutAsync(userToVerify);
            if (looked)
            {
                ModelState.AddModelError("login_failure", $"Number of your login attempts expired, try again in {userToVerify.LockoutEnd}");
                return await Task.FromResult<ClaimsIdentity>(null);
            }

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                // Ensure the email is confirmed.
                if (!await _userManager.IsEmailConfirmedAsync(userToVerify))
                {
                    ModelState.AddModelError("login_failure", "Email not confirmed");
                    return await Task.FromResult<ClaimsIdentity>(null);
                }
                await _userManager.ResetAccessFailedCountAsync(userToVerify);
                return await Task.FromResult(await _jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

            //inc the number of failed logins
            await _userManager.AccessFailedAsync(userToVerify);
            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }

        [AllowAnonymous]
        [HttpGet("confirm", Name = "ConfirmEmail")]
        public async Task<IActionResult> Confirm(string uid, string token)
        {
            var user = await _userManager.FindByIdAsync(uid);
            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
            //change links
            if (confirmResult.Succeeded)
            {
                //return Redirect("/?confirmed=1");
                return Ok("email_confirmed");
            }
            else
            {
                //    return Redirect("/error/email-confirm");
                return BadRequest();
            }
        }
        [Produces(contentType: "application/json")]
        [ProducesResponseType(204)]
        [HttpPost("restore", Name = "RestorePassword")]
        public async Task<IActionResult> RestorePassword([FromBody]RestorePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                ModelState.AddModelError(nameof(user.Email), "Email not confirmed or user doesn't exist");
                return BadRequest(ModelState);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.RouteUrl("ResetPassword",
                 new { uid = user.Id, token }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Reset Password",
                $"Please reset your password by clicking here:  <a href=\"{callbackUrl}\">link</a>");
            return NoContent();
        }

        [HttpGet("reset", Name = "ResetPassword")]
        public IActionResult ResetPassword(string uid, string token)
        {
            //probably redirect to website
            return Ok(token);
        }
        [Produces(contentType: "application/json")]
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody]SetPasswordDto setPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userManager.FindByEmailAsync(setPasswordDto.Email);

            if (user == null)
            {
                return NotFound();
            }

            var resetResult = _userManager.ResetPasswordAsync(user, setPasswordDto.Token, setPasswordDto.Password).Result;
            
            if (!resetResult.Succeeded)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            var ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"];

            if (refreshToken == null)
                return BadRequest();

            var res = await _jwtFactory.RefreshToken(refreshToken, _jwtOptions,ip, userAgent);

            if (res == null)
                return BadRequest();
            return Ok(res);
        }
       
        [Authorize]
        [ProducesResponseType(204)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.Id)?.Value;

            if (id == null)
                return NotFound();

            await _jwtFactory.RemoveRefreshTokens(id);

            return NoContent();
        }

    }
}
