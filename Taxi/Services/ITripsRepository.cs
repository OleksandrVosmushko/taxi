using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Helpers;
using Taxi.Models;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public interface ITripsRepository
    {
       // bool SetTrip(Trip trip);

        Task<List<TripHistoryRouteNode>> GetTripRouteNodes(Guid tripId);

        void RemoveTrip(Guid customerId);
        
        Trip GetTrip(Guid customerId, bool includeRoutes = false);

   //     bool UpdateTripLocation(double lon, double lat, Guid customerId);

        PagedList<TripDto> GetNearTrips(double lon, double lat, PaginationParameters paginationParameters);

        Trip GetTripByDriver(Guid driverId, bool includeRoutes  = false);
        
        Task AddTripHistory(TripHistory tripHistory);

        Task<TripHistory> GetTripHistory(Guid id);

        PagedList<TripHistory> GetTripHistoriesForCustomer(Guid CustomerId, TripHistoryResourceParameters resourceParameters);

        PagedList<TripHistory> GetTripHistoriesForDriver(Guid DriverId, TripHistoryResourceParameters resourceParameters);
        Task<bool> UpdateTrip(Trip trip, PlaceDto from = null, PlaceDto to = null);
        Task AddNode(TripRouteNode node);
        void InsertTrip(Trip tripEntity, double lat1, double lon1, double lat2, double lon2);
        void AddRefundRequest(RefundRequest refundRequest);
        void AddContract(Contract contract);

        Contract GetContract(ulong id);
    }
}
   