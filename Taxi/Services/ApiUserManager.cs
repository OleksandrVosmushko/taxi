using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Taxi.Entities;

namespace Taxi.Services
{
    public class ApiUserManager : UserManager<AppUser> 
    {
        public ApiUserManager(IUserStore<AppUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<AppUser> passwordHasher,
            IEnumerable<IUserValidator<AppUser>> userValidators,
            IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services, ILogger<UserManager<AppUser>> logger) : 
            base(store, optionsAccessor, passwordHasher,
                userValidators, passwordValidators, keyNormalizer,
                errors, services, logger){ }

        public override async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            if (Users.Any(u => u.PhoneNumber == user.PhoneNumber))
            {
                return IdentityResult.Failed(new IdentityError()
                {
                    Code = "DuplicatePhoneException", // Wrong practice, lets set some beautiful code values in the future
                    Description = "An existing user with same phone already exists."
                });
            }
            return await base.CreateAsync(user, password);
        }

    }
}
