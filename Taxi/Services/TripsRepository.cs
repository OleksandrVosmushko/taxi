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
        private ITripsLocationRepository _locationRepository;

        public TripsRepository(ApplicationDbContext dataContext, ITripsLocationRepository locationRepository)
        {
            _dataContext = dataContext;
            _locationRepository = locationRepository;
        }
        public Trip GetTrip(Guid customerId)
        {
            var trip = _locationRepository.GetTripStartLocation(customerId);

            if (trip != null)
                return trip;

            trip = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);
            
            return trip;
        }

        public void RemoveTrip(Guid customerId)
        {
            var tripToRemove = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);

            if (tripToRemove != null)
            {
                _dataContext.Trips.Remove(tripToRemove);
                _locationRepository.RemoveTripLocation(customerId);
            }
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

                _locationRepository.SetLastTripLocation(trip.CustomerId, trip);   
            } catch (Exception e)
            {

                return false;
            }
            return true;
        }

        public bool UpdateTripLocation(double lon, double lat, Guid customerId)
        {
            var trip = _locationRepository.GetTripStartLocation(customerId);
            if (trip == null)
            {
                trip = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);
            }

            if (trip == null)
                return false;
        
            var from = trip.Places.FirstOrDefault(p => p.IsFrom == true);
            from.Latitude = lat;
            from.Longitude = lon;
            _locationRepository.SetLastTripLocation(customerId, trip);
            return true;
        }
    }
}
