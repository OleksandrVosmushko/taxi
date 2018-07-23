using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;

namespace Taxi.Services
{
    public class TripsInMemoryStorage : ITripsRepository
    {
        private IMemoryCache _memoryCache;



        public TripsInMemoryStorage(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        public bool AddTrip(Trip trip)
        {
            try
            {
                _memoryCache.Set(generateTripKey(trip.CustomerId), trip, TimeSpan.FromMinutes(10));
            }
            catch {
                return false;
            }
            return true;
        }

        public void RemoveTrip ( Guid customerId)
        {
            _memoryCache.Remove(generateTripKey(customerId));
        }

        public Trip GetTrip(Guid customerId)
        {
            return _memoryCache.Get<Trip>(generateTripKey(customerId));
        }
        
        private string generateTripKey(Guid customerId)
        {
            return "trip" + customerId.ToString();
        }
    }
}
