using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Models.Trips
{
    public struct LastTripLocation
    {
        public double Lon;
        public double Lat;
        public DateTime DatabaseUpdate;
        public DateTime LastUpdate;
    }
}
