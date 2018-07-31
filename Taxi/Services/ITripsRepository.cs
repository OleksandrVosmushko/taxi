using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public interface ITripsRepository
    {
        bool SetTrip(Trip trip);

        void RemoveTrip(Guid customerId);
        
        Trip GetTrip(Guid customerId);

        bool UpdateTripLocation(double lon, double lat, Guid customerId);

        List<TripDto> GetNearTrips(double lon, double lat);
    }
}
