using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class FinishTripPlace
    {
        public Guid Id { get; set; }

        public Guid TripHistoryId { get; set; }

        public bool IsFrom { get; set; }

        public bool IsTo { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public TripHistory TripHistory { get; set; }
    }
}
