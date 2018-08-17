using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Helpers;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public interface ITripsRepository
    {
        bool SetTrip(Trip trip);

        Task<List<TripRouteNode>> GetTripRouteNodes(Guid tripId);

        void RemoveTrip(Guid customerId);
        
        Trip GetTrip(Guid customerId);

        bool UpdateTripLocation(double lon, double lat, Guid customerId);

        List<TripDto> GetNearTrips(double lon, double lat);

        Trip GetTripByDriver(Guid driverId, bool includeRoutes  = false);
        
        Task AddTripHistory(TripHistory tripHistory);

        Task<TripHistory> GetTripHistory(Guid id);

        PagedList<TripHistory> GetTripHistoriesForCustomer(Guid CustomerId, TripHistoryResourceParameters resourceParameters);

        PagedList<TripHistory> GetTripHistoriesForDriver(Guid DriverId, TripHistoryResourceParameters resourceParameters);

        Task UpdateTrip(Trip trip);

        Task AddNode(TripRouteNode node);
    }
}
