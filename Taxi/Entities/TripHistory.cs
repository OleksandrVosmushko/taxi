using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class TripHistory
    {
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public Customer Customer { get; set; }

        public Guid DriverId { get; set; }

        public Driver Driver { get; set; }

        public Place From { get; set; }

        public Place To { get; set; }

        public DateTime EndTime { get; set; }
    }
}
