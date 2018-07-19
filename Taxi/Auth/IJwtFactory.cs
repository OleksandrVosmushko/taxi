using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Taxi.Models;

namespace Taxi.Auth
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(string userName, ClaimsIdentity claimsIdentity);
        Task<ClaimsIdentity> GenerateClaimsIdentity(string userName, string id);
        Task<string> GenerateRefreshToken(string userName, ClaimsIdentity claimsIdentity);
        Task<TokensDto> RefreshToken(string refreshToken, JwtIssuerOptions jwtOptions);
        Task RemoveRefreshTokens(string userId);
    }
}
