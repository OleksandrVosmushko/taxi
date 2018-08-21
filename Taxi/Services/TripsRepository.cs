﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
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

        public void InsertTrip(Trip trip, double lat1, double lon1,double lat2,double lon2)
        {
            var query = string.Format(System.IO.File.ReadAllText("InsertTripQuery.txt"), lon1, lat1, lon2, lat2);
            
            List<NpgsqlParameter> sqlParameters = new List<NpgsqlParameter>();
            if (trip.Id == default(Guid))
                trip.Id = Guid.NewGuid();
            sqlParameters.Add(new NpgsqlParameter("Id", trip.Id));
            sqlParameters.Add(new NpgsqlParameter("CustomerId", trip.CustomerId));
            var did =(object) trip.DriverId ?? DBNull.Value;
            sqlParameters.Add(new NpgsqlParameter("DriverId", did));
            sqlParameters.Add(new NpgsqlParameter("LastLat", trip.LastLat));
            sqlParameters.Add(new NpgsqlParameter("LastLon", trip.LastLon));
            sqlParameters.Add(new NpgsqlParameter("Distance", trip.Distance));
            sqlParameters.Add(new NpgsqlParameter("LastUpdateTime", trip.LastUpdateTime));
            sqlParameters.Add(new NpgsqlParameter("CreationTime", trip.CreationTime));
            sqlParameters.Add(new NpgsqlParameter("DriverTakeTripTime", trip.DriverTakeTripTime));
            sqlParameters.Add(new NpgsqlParameter("StartTime", trip.StartTime));
            sqlParameters.Add(new NpgsqlParameter("FinishTime", trip.FinishTime));
            //sqlParameters.Add(new NpgsqlParameter("lon1", lon1));
            //sqlParameters.Add(new NpgsqlParameter("lat1", lat1));
            //sqlParameters.Add(new NpgsqlParameter("lon2", lon2));
            //sqlParameters.Add(new NpgsqlParameter("lat2", lat2));
            _dataContext.Database.ExecuteSqlCommand(query, sqlParameters);
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
            var query = string.Format(System.IO.File.ReadAllText("GetNearQuery.txt"));
            List<NpgsqlParameter> sqlParameters = new List<NpgsqlParameter>();
            sqlParameters.Add(new NpgsqlParameter("lon", lon));
            sqlParameters.Add(new NpgsqlParameter("lat", lat));
            sqlParameters.Add(new NpgsqlParameter("items", 10));
            sqlParameters.Add(new NpgsqlParameter("page", 1));
            var trips = _dataContext.Trips.FromSql(query, sqlParameters.ToArray()).ToList();
            
            //var trips = _dataContext.Trips.Where(p => p.DriverId == null)
            //    .OrderBy(d => d.From.Distance(Helpers.Location.pointFromLatLng(lat, lon)))
            //    .ToList();


            var tripsDto = new List<TripDto>();

            foreach (var t in trips)
            {
                
                var customer = _userRepository.GetCustomerById(t.CustomerId);
                
                tripsDto.Add(new TripDto() {
                    CustomerId = customer.Id,
                    FirstName = customer.Identity.FirstName,
                    LastName = customer.Identity.LastName,
                    From = Helpers.Location.PointToPlaceDto(t.From),
                    To = Helpers.Location.PointToPlaceDto(t.To)
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

        //public async Task UpdateTrip(Trip trip)
        //{
        //    _locationRepository.SetLastTripLocation(trip.CustomerId, trip);

        //    await _dataContext.SaveChangesAsync();
        //}

        public async Task AddNode(TripRouteNode node)
        {
            await _dataContext.TripRouteNodes.AddAsync(node);
            await _dataContext.SaveChangesAsync();
        }
        public Trip GetTripByDriver(Guid driverId, bool includeRoutes = false)
        {
            if (!includeRoutes)
            {
                return _dataContext.Trips.FirstOrDefault(t => t.DriverId == driverId);
            }
            else
            {
                return _dataContext.Trips.Include(tr => tr.RouteNodes).FirstOrDefault(t => t.DriverId == driverId);
            }
        }

        public PagedList<TripHistory> GetTripHistoriesForCustomer(Guid CustomerId, TripHistoryResourceParameters resourceParameters)
        {
            var beforePaging = _dataContext.TripHistories.Where(t => t.CustomerId == CustomerId)
                .OrderByDescending(h => h.FinishTime);
            return PagedList<TripHistory>.Create(beforePaging, resourceParameters.PageNumber, resourceParameters.PageSize);
        }

        public PagedList<TripHistory> GetTripHistoriesForDriver(Guid DriverID, TripHistoryResourceParameters resourceParameters)
        {
            var beforePaging = _dataContext.TripHistories.Where(t => t.DriverId == DriverID)
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

        public async Task<bool> UpdateTrip(Trip trip, PlaceDto from=null , PlaceDto to = null)
        {
            try
            {
                _dataContext.Update(trip);

                _dataContext.SaveChanges();

                if (from != null)
                {
                    var queryfrom = string.Format(System.IO.File.ReadAllText("UpdateFromQuery.txt"));

                    List<NpgsqlParameter> sqlParameters = new List<NpgsqlParameter>();

                    sqlParameters.Add(new NpgsqlParameter("lon", from.Longitude));

                    sqlParameters.Add(new NpgsqlParameter("lat", from.Latitude));

                    sqlParameters.Add(new NpgsqlParameter("CustomerId", trip.CustomerId));

                    await _dataContext.Database.ExecuteSqlCommandAsync(queryfrom, sqlParameters);
                }

                if (to != null)
                {
                    var queryto = string.Format(System.IO.File.ReadAllText("UpdateToQuery.txt"));

                    List<NpgsqlParameter> sqlParameters = new List<NpgsqlParameter>();

                    sqlParameters.Add(new NpgsqlParameter("lon", to.Longitude));

                    sqlParameters.Add(new NpgsqlParameter("lat", to.Latitude));

                    sqlParameters.Add(new NpgsqlParameter("CustomerId", trip.CustomerId));

                    await _dataContext.Database.ExecuteSqlCommandAsync(queryto, sqlParameters);
                }
                _dataContext.Entry(trip).Reload();
            }
            catch(Exception e)
            {
                
                return false;
            }

            return true;
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

            var from = trip.From;
            from = Helpers.Location.pointFromLatLng(lat, lon);
           // _locationRepository.SetLastTripLocation(customerId, trip);
            return true;
        }
    }
}
