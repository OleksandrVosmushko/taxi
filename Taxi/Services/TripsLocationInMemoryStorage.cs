using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public class TripsLocationInMemoryStorage : ITripsLocationRepository
    {
        private IMemoryCache _cache;

        public TripsLocationInMemoryStorage(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Trip GetTripStartLocation(Guid customerId)
        {
            return _cache.Get<Trip>(generateTripKey(customerId));
        }

        public void RemoveTripLocation(Guid customerId)
        {
            _cache.Remove(generateTripKey(customerId));
        }

        public void SetLastTripLocation(Guid customerId, Trip location)
        {
            _cache.Set<Trip>(generateTripKey(customerId), location);
        }

        private string generateTripKey(Guid customerId)
        {
            return "trip_loc" + customerId.ToString();
        }
    }
}
