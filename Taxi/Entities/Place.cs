using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Entities
{
    public class Place
    {
        public Guid id { get; set; }

        public Guid TripId { get; set; }

        public bool IsFrom { get; set; }

        public bool IsTo { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Trip Trip { get; set; }
    }
}
