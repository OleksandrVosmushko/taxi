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

        public List<FinishTripPlace> Places { get; set; }
        
        public DateTime CreationTime { get; set; }

        public DateTime DriverTakeTripTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        public Decimal Price { get; set; }

        public double Distance { get; set; }

        public List<TripRouteNode> RouteNodes { get; set; } = new List<TripRouteNode>();
    }
}
