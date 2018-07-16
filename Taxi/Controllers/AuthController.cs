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
    public class AuthController : Controller
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



        [HttpGet()]
        [Authorize(Policy = "Customer")]
        public IActionResult GetAuthorizedOnly()
        {
            return Ok(1);
        }
        [Produces(contentType: "application/json")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]CreditionalsDto credentials)
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

            var jwt = await Tokens.GenerateJwt(identity, _jwtFactory, credentials.UserName, _jwtOptions, new JsonSerializerSettings { /*Formatting = Formatting.Indented*/ });

            return Ok(JsonConvert.DeserializeObject(jwt)); ;
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
                return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
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
        public async Task<IActionResult> RestorePassword(RestorePasswordDto model)
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
        public async Task<IActionResult> ResetPassword(SetPasswordDto setPasswordDto)
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
    }
}
