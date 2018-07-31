﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Taxi.Data;
using Taxi.Entities;
using Taxi.Models.Trips;

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

        public List<TripDto> GetNearTrips(double lon, double lat)
        {
            return _locationRepository.GetNearTrips(lon, lat);
        }

        public Trip GetTrip(Guid customerId)
        {
            var trip = _locationRepository.GetTripStartLocation(customerId);

            if (trip != null)
                return trip;

            trip = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);
            
            return trip;
        }

        public Trip GetTripByDriver(Guid driverId)
        {
            var trip = _dataContext.Trips.FirstOrDefault(t => t.DriverId == driverId);

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
                if (trip.Id == default(Guid))
                {
                    _dataContext.Add(trip);
                }
                else _dataContext.Update(trip);

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
