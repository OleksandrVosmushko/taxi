using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public interface ITripsLocationRepository
    {
        Trip  GetTripStartLocation(Guid customerId);

        void SetLastTripLocation(Guid customerId, Trip location);

        void RemoveTripLocation(Guid customerId);

        List<TripDto> GetNearTrips(double lon, double lat);
    }
}
