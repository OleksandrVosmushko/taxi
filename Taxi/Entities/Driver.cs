using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class Driver
    {
        public Guid Id { get; set; }

        public string ConnectionId { get; set; }

        public string IdentityId { get; set; }

        public AppUser Identity { get; set; }

        public string City { get; set; }

        public Trip CurrentTrip { get; set; }

        public Vehicle Vehicle { get; set; }

        public List<TripHistory> TripHistories { get; set; } = new List<TripHistory>();

        public DriverLicense DriverLicense { get; set; }

    }
}
