using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;

namespace Taxi.Services
{
    public interface ITripsRepository
    {
        bool AddTrip(Trip trip);

        void RemoveTrip(Guid customerId);
        
        Trip GetTrip(Guid customerId);
        
    }
}
