using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Taxi.Models;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity;
using Taxi.Entities;
using Taxi.Services;

namespace Taxi.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IUsersRepository _repository;
        private readonly UserManager<AppUser> _userManager;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions, IUsersRepository repository, UserManager<AppUser> userManager)
        {
            _jwtOptions = jwtOptions.Value;
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity claimsIdentity)
        {
            var claims = new[]
            {
                new Claim (JwtRegisteredClaimNames.Sub, userName),
                new Claim (JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                claimsIdentity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Rol),
                claimsIdentity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
            };

     
            var jwt = new JwtSecurityToken(
                 issuer: _jwtOptions.Issuer,
                 audience: _jwtOptions.Audience,
                 claims: claims,
                 notBefore: _jwtOptions.NotBefore,
                 expires: _jwtOptions.Expiration,
                 signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() -
                new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalSeconds);

        public async Task <ClaimsIdentity> GenerateClaimsIdentity(string userName, string id)
        {
            
            var user = await _userManager.FindByIdAsync(id);

            var rolClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.Rol);

            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Id, id),
                rolClaim
            }); ;

            return identity;
        }

    }
}
