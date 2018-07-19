﻿using Microsoft.Extensions.Options;
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
using Taxi.Helpers;

namespace Taxi.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IUsersRepository _repository;
        private readonly UserManager<AppUser> _userManager;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions,
            IUsersRepository repository,
            UserManager<AppUser> userManager)
        {
            _jwtOptions = jwtOptions.Value;
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity claimsIdentity )
        {
            var claims = new List<Claim>
            {
                new Claim (JwtRegisteredClaimNames.Sub, userName),
                new Claim (JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                claimsIdentity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
            };
            foreach (var claim in claimsIdentity.Claims)
            {
                if (claim.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.Rol)
                {
                    claims.Add(claim);
                }
            }

     
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

            var rolClaims = (await _userManager.GetClaimsAsync(user)).Where(c => c.Type == Helpers.Constants.Strings.JwtClaimIdentifiers.Rol);

            ClaimsIdentity identity = new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Id, id)  
            }.Union(rolClaims)); 

            return identity;
        }

        public async Task<string> GenerateRefreshToken(string userName, ClaimsIdentity claimsIdentity)
        {
            var claims = new List<Claim>
            {
                new Claim (JwtRegisteredClaimNames.Sub, userName),
                new Claim (JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                claimsIdentity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id)
            };
            
            var jwt = new JwtSecurityToken(
                 issuer: _jwtOptions.Issuer,
                 audience: _jwtOptions.Audience,
                 claims: claims,
                 notBefore: _jwtOptions.NotBefore,
                 expires: _jwtOptions.RefleshExpiration,
                 signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var hashedJwt = _userManager.PasswordHasher.HashPassword(new AppUser(), encodedJwt);

            await _repository.AddRefreshToken(new Entities.RefreshToken()
            {
                Token = hashedJwt,
                IdentityId = claimsIdentity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id).Value,
                Expiration = ToUnixEpochDate(_jwtOptions.RefleshExpiration)
            });

            var tokensFromDb = _repository.GetTokensForUser(claimsIdentity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id).Value);

            foreach (var t in tokensFromDb.ToList())
            {
                if (t.Expiration < ToUnixEpochDate(DateTime.UtcNow))
                {
                    await _repository.DeleteRefleshToken(t);
                }
            }

            return encodedJwt;
        }

        public async Task<TokensDto> RefreshToken(string refreshToken, JwtIssuerOptions jwtOptions)
        {
            var handler = new JwtSecurityTokenHandler();

            var tokenClaims = (handler.ReadToken(refreshToken) as JwtSecurityToken).Claims;

            var expirationTime = tokenClaims.FirstOrDefault(o => o.Type == "exp").Value;

            if (expirationTime == null)
            {
                return null;
            }
            var date = ToUnixEpochDate(DateTime.UtcNow);
            if (long.Parse(expirationTime) < ToUnixEpochDate(DateTime.UtcNow))
            {
                return null;
            }

            var uid = tokenClaims.FirstOrDefault(o => o.Type == "id").Value;

            if (uid == null)
            {
                return null;
            }

            var tokensFromDb = _repository.GetTokensForUser(uid);
            
            var curToken = tokensFromDb.Where(t => (_userManager.PasswordHasher
                .VerifyHashedPassword(new AppUser(), t.Token, refreshToken)) == PasswordVerificationResult.Success)
                .SingleOrDefault();

            if (curToken == null)
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(curToken.IdentityId);
            
            if (tokensFromDb.Count() > 20)
            {
                foreach (var t in tokensFromDb.ToList())
                {
                    if (t!= null)
                        await _repository.DeleteRefleshToken(t);
                }
            } else
            {
                if (curToken != null)
                    await _repository.DeleteRefleshToken(curToken);
            }


            if (user == null)
            {
                return null;
            }

            var claimsIdentity = await GenerateClaimsIdentity(user.UserName, user.Id);

            var newRefreshToken = await GenerateRefreshToken(user.UserName, claimsIdentity);

            var newAccessToken =  await GenerateEncodedToken(user.UserName, claimsIdentity);

            var responce = new TokensDto()
            {
                expires_in = (int)jwtOptions.ValidFor.TotalSeconds,
                auth_token = newAccessToken,
                refresh_token = newRefreshToken
            };

            return responce;
        }
    }
}
