using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Data;
using Taxi.Entities;
using Taxi.Helpers;
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
            //probably change it
            //var trips = _dataContext.Places.Where(p => p.IsFrom == true)
            //    .OrderBy(d => d.Location.Distance(Helpers.Location.pointFromLatLng(lat, lon))).Include(t=> t.Trip);
            var trips = _dataContext.Trips.Where(p => p.DriverId == null)
                .OrderBy(d => d.Places.FirstOrDefault(p => p.IsFrom == true).Location.Distance(Helpers.Location.pointFromLatLng(lat, lon)))
                .Include(l => l.Places).ToList();

            var tripsDto = new List<TripDto>();

            foreach (var t in trips)
            {
                
                var customer = _userRepository.GetCustomerById(t.CustomerId);
                
                tripsDto.Add(new TripDto() {
                    CustomerId = customer.Id,
                    FirstName = customer.Identity.FirstName,
                    LastName = customer.Identity.LastName,
                    From = Helpers.Location.CartesianToSpherical(t.Places.FirstOrDefault(p => p.IsFrom == true).Location),
                    To = Helpers.Location.CartesianToSpherical(t.Places.FirstOrDefault(p => p.IsTo == true).Location)
                });
            }
            return tripsDto;
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
            _locationRepository.SetLastTripLocation(trip.CustomerId, trip);

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

        public PagedList<TripHistory> GetTripHistoriesForCustomer(Guid CustomerId, TripHistoryResourceParameters resourceParameters)
        {
            var beforePaging = _dataContext.TripHistories.Where(t => t.CustomerId == CustomerId)
                .Include(o=>o.Places)
                .OrderByDescending(h => h.FinishTime);
            return PagedList<TripHistory>.Create(beforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public PagedList<TripHistory> GetTripHistoriesForDriver(Guid DriverID, TripHistoryResourceParameters resourceParameters)
        {
            var beforePaging = _dataContext.TripHistories.Where(t => t.DriverId == DriverID)
                .Include(o => o.Places)
                .OrderByDescending(h => h.FinishTime);
            return PagedList<TripHistory>.Create(beforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public async Task<TripHistory> GetTripHistory(Guid id)
        {
            return await _dataContext.TripHistories.FindAsync(id);
        }

        public async Task<List<TripHistoryRouteNode>> GetTripRouteNodes(Guid tripHistoryId)
        {
            return await _dataContext.TripHistoryRouteNodes.Where(n=>n.TripHistoryId == tripHistoryId)
                .OrderBy(o=>o.UpdateTime).ToListAsync();
        }

        public void RemoveTrip(Guid customerId)
        {
            var tripToRemove = _dataContext.Trips.FirstOrDefault(t => t.CustomerId == customerId);

            if (tripToRemove != null)
            {
                _dataContext.Trips.Remove(tripToRemove);
                _locationRepository.RemoveTripLocation(customerId);
                _dataContext.SaveChanges();
            }
           
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
            from.Location = Helpers.Location.pointFromLatLng(lat, lon);
           // _locationRepository.SetLastTripLocation(customerId, trip);
            return true;
        }
    }
}
