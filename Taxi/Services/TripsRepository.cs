using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Data;
using Taxi.Entities;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public class TripsRepository : ITripsRepository
    {
        private ApplicationDbContext _dataContext;
        private ITripsLocationRepository _locationRepository;
        private IUsersRepository _userRepository;

        public TripsRepository(ApplicationDbContext dataContext, ITripsLocationRepository locationRepository, IUsersRepository usersRepository)
        {
            _dataContext = dataContext;
            _locationRepository = locationRepository;
            _userRepository = usersRepository;
        }
        
        public async Task AddTripHistory(TripHistory tripHistory)
        {
            await _dataContext.TripHistories.AddAsync(tripHistory);

            await _dataContext.SaveChangesAsync();
        }

        public List<TripDto> GetNearTrips(double lon, double lat)
        {
            var trips =_locationRepository.GetNearTrips(lon, lat);
            foreach (var t in trips)
            {
                var customer = _userRepository.GetCustomerById(t.CustomerId);

                t.LastName = customer.Identity.LastName;
                t.FirstName = customer.Identity.FirstName;
            }
            return trips;
        }

        public Trip GetTrip(Guid customerId)
        {
            var trip = _locationRepository.GetTripStartLocation(customerId);

            if (trip != null)
                return trip;

            trip = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);
            
            return trip;
        }

        public async Task UpdateTrip(Trip trip)
        {
            await _dataContext.SaveChangesAsync();
        }

        public async Task AddNode(TripRouteNode node)
        {
            await _dataContext.TripRouteNodes.AddAsync(node);
            await _dataContext.SaveChangesAsync();
        }
        public Trip GetTripByDriver(Guid driverId, bool includeRoutes = false)
        {
            if (!includeRoutes)
            {
                return _dataContext.Trips.Include(t => t.Places).FirstOrDefault(t => t.DriverId == driverId);
            }
            else
            {
                return _dataContext.Trips.Include(t => t.Places).Include(tr => tr.RouteNodes).FirstOrDefault(t => t.DriverId == driverId);
            }
        }

        public async Task<List<TripHistory>> GetTripHistoriesForCustomer(Guid CustomerId)
        {
            return await _dataContext.TripHistories.Where(t => t.CustomerId == CustomerId).Include(o=>o.Places).OrderByDescending(h => h.FinishTime).ToListAsync();
        }

        public async Task<List<TripHistory>> GetTripHistoriesForDriver(Guid DriverID)
        {
            return await _dataContext.TripHistories.Where(t => t.DriverId == DriverID).Include(o => o.Places).OrderByDescending(h => h.FinishTime).ToListAsync();
        }

        public async Task<TripHistory> GetTripHistory(Guid id)
        {
            return await _dataContext.TripHistories.FindAsync(id);
        }

        public async Task<List<TripRouteNode>> GetTripRouteNodes(Guid tripHistoryId)
        {
            return await _dataContext.TripRouteNodes.Where(n=>n.TripHistoryId == tripHistoryId)
                .OrderByDescending(o=>o.UpdateTime).ToListAsync();
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
                    var tr = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == trip.CustomerId);
                    if (tr != null)
                    {
                        _dataContext.Remove(tr);
                        _dataContext.SaveChanges();
                        _dataContext.Add(trip);
                    } else
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
