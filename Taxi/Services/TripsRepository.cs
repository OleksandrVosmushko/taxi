using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Taxi.Data;
using Taxi.Entities;

namespace Taxi.Services
{
    public class TripsRepository : ITripsRepository
    {
        private ApplicationDbContext _dataContext;
        
        public TripsRepository(ApplicationDbContext dataContext)
        {
            _dataContext = dataContext;
        }
        public Trip GetTrip(Guid customerId)
        {
            var trip = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);

            return trip;
        }

        public void RemoveTrip(Guid customerId)
        {
            var tripToRemove = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);

            if (tripToRemove != null)
                _dataContext.Trips.Remove(tripToRemove);
            _dataContext.SaveChanges();
        }

        public bool SetTrip(Trip trip)
        {
            try
            {
                _dataContext.Entry(trip).State = trip.Id == default(Guid) ?
                                   EntityState.Added :
                                   EntityState.Modified;
                _dataContext.SaveChanges();
            } catch
            {
                return false;
            }
            return true;
        }
        

    }
}
