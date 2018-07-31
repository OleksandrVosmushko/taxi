using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class Driver
    {
        public Guid Id { get; set; }

        public string IdentityId { get; set; }

        public AppUser Identity { get; set; }

        public string City { get; set; }

        public Trip CurrentTrip { get; set; }
        
    }
}
