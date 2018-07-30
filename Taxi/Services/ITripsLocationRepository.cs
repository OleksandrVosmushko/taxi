using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public interface ITripsLocationRepository
    {
        LastTripLocation  GetTripStartLocation(Guid customerId);

        void SetLastTripLocation(Guid customerId, LastTripLocation location);
        
        
    }
}
