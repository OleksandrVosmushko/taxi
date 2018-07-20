using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Services
{
    public class TripsInMemoryStorage : ITripsRepository
    {
        private IMemoryCache _memoryCache;



        public TripsInMemoryStorage(IMemoryCache cache)
        {
            _memoryCache = cache;
        }


    }
}
