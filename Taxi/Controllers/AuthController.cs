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

namespace Taxi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController: Controller
    {
        private UserManager<AppUser> _userManager;
        private IJwtFactory _jwtFactory;
        private JwtIssuerOptions _jwtOptions;
        private IEmailSender _emailSender;

        public AuthController(UserManager<AppUser> userManager, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions, IEmailSender emailSender)
        {
            _userManager = userManager;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _emailSender = emailSender;
        }

        [HttpGet("email")]
        public async Task<IActionResult> SendEmail()
        {
            await _emailSender.SendEmailAsync("sasha.vosmushko@gmail.com", "no subject" , "message", "message");
            return Ok();
        }

        [HttpGet()]
        [Authorize (Policy = "Customer")]
        public IActionResult GetAuthorizedOnly()
        {
            return Ok(1);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]CustomerCreditionalsDto credentials)
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
            
            var jwt = await Tokens.GenerateJwt(identity, _jwtFactory, credentials.UserName, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return Ok(jwt);
        }
        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);
            
            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);
            
            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                // Ensure the email is confirmed.
                if (!await _userManager.IsEmailConfirmedAsync(userToVerify))
                {
                    ModelState.AddModelError("login_failure", "Email not confirmed");
                    return await Task.FromResult<ClaimsIdentity>(null);
                }
                return await Task.FromResult( _jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

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
                return Redirect("/?confirmed=1");
            }
            else
            {
                return Redirect("/error/email-confirm");
            }
        }
    }
}
