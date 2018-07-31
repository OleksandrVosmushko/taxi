using Google.Common.Geometry;
using IntervalTree;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Entities;
using Taxi.Models.Trips;

namespace Taxi.Services
{
    public class TripsLocationInMemoryStorage : ITripsLocationRepository
    {

        private static int _level = 13;
        private static IntervalTree<UserList> rtree = new IntervalTree<UserList>();
        private static ConcurrentDictionary<Guid, S2CellId> _currentUsersLocations = new ConcurrentDictionary<Guid, S2CellId>();
        private static ConcurrentDictionary<Guid, S2CellId> _accurateUsersLocations = new ConcurrentDictionary<Guid, S2CellId>();

        static object locker = new object();

        private static ConcurrentDictionary<Guid, Trip> _accurateTripsLocations = 
            new ConcurrentDictionary<Guid, Trip>();

        public TripsLocationInMemoryStorage()
        {

        }

        public Trip GetTripStartLocation(Guid customerId)
        {
            return _accurateTripsLocations[customerId];
        }

        public void RemoveTripLocation(Guid customerId)
        {
            lock (locker)
            {
                var removeOutParameter = new Trip();
                _accurateTripsLocations.TryRemove(customerId, out removeOutParameter);
                RemoveUser(customerId);
            }
        }

        public void SetLastTripLocation(Guid customerId, Trip location)
        {
            lock (locker) {
                var from = location.Places.FirstOrDefault(p => p.IsFrom == true);
                bool res = UpdateUser(customerId, from.Longitude, from.Latitude);
                _accurateTripsLocations[customerId] = location;
            }
        }
        
        public List<TripDto> GetNearTrips(double lon, double lat)
        {
            lock (locker)
            {
                var res = new List<TripDto>();
                var qres = Search(lon, lat, 5000);

                foreach (var r in qres)
                {
                    var curTrip = _accurateTripsLocations[r];
                    var from = curTrip.Places.FirstOrDefault(c => c.IsFrom == true);
                    var to = curTrip.Places.FirstOrDefault(c => c.IsTo == true);
                    res.Add(new TripDto
                    {
                        CustomerId = r,
                        From = new PlaceDto { Latitude = from.Latitude, Longitude = from.Longitude},
                        To = new PlaceDto { Latitude = to.Latitude, Longitude = to.Longitude }
                    });
                }
                return res;
            }
        }

        private struct UserList : IComparable<UserList>
        {
            public S2CellId s2CellId;

            public List<Guid> list;

            public int CompareTo(UserList other)
            {
                return s2CellId.CompareTo(other.s2CellId);
            }
        }

        private bool UpdateUser(Guid uid, double lon, double lat)
        {
            lock (locker)
            {
                var lonLat = S2LatLng.FromDegrees(lat, lon);

                var cellId = S2CellId.FromLatLng(lonLat);

                var cellIdStorageLevel = cellId.ParentForLevel(_level);

                _accurateUsersLocations[uid] = cellId;

                if (_currentUsersLocations.ContainsKey(uid))
                {
                    var oldCell = _currentUsersLocations[uid];

                    if (oldCell == cellIdStorageLevel)
                    {
                        return true;
                    }

                    var query_res = rtree.Search(new UserList() { s2CellId = oldCell });

                    if (query_res.Count > 0)
                    {
                        //remove from old cell
                        foreach (var item in query_res)
                        {
                            item.Start.list.Remove(uid);
                            if (item.Start.list.Count == 0)
                            {
                                rtree.Remove(item);
                            }
                        }
                    }
                }
                //if there is no key
                _currentUsersLocations[uid] = cellIdStorageLevel;
                var users = new List<Guid>();

                var query_res2 = rtree.Search(new UserList() { s2CellId = cellIdStorageLevel });

                if (query_res2.Count > 0)
                {
                    foreach (var item in query_res2)
                    {
                        item.Start.list.Add(uid);
                    }
                    return true;
                }
                users.Add(uid);

                var toinsert = new UserList() { s2CellId = cellIdStorageLevel, list = users };

                rtree.Add(new Interval<UserList>() { Start = toinsert, End = toinsert });
                return true;
            }
        }

        private List<Guid> Search(double lon, double lat, int radius)
        {
            lock (locker)
            {
                var latlng = S2LatLng.FromDegrees(lat, lon);

                var centerPoint = pointFromLatLng(lat, lon);

                var centerAngle = ((double)radius) / EarthRadiusM;

                var cap = S2Cap.FromAxisAngle(centerPoint, S1Angle.FromRadians(centerAngle));

                var regionCoverer = new S2RegionCoverer();

                regionCoverer.MaxLevel = 13;
                
                var covering = regionCoverer.GetCovering(cap);
                
                var res = new List<Guid>();
                
                foreach (var u in covering)
                {
                    var sell = new S2CellId(u.Id);

                    if (sell.Level < _level)
                    {
                        var begin = sell.ChildBeginForLevel(_level);
                        var end = sell.ChildEndForLevel(_level);

                        var qres = rtree.Search(new Interval<UserList>(new UserList() { s2CellId = begin }, new UserList() { s2CellId = end }));


                        foreach (var item in qres)
                        {

                            res.AddRange(item.Start.list);
                        }
                    }
                    else
                    {
                        var qres = rtree.Search(new UserList() { s2CellId = sell });
                        if (qres.Count > 0)
                        {
                            foreach (var r in qres)
                            {
                                res.AddRange(r.Start.list);
                            }
                        }
                    }
                }
                return res;
            }
        }


        private bool RemoveUser(Guid uid)
        {
            lock (locker)
            {
                if (!_currentUsersLocations.ContainsKey(uid))
                    return false;
                var cell = _currentUsersLocations[uid];

                var qres = rtree.Search(new Interval<UserList>(new UserList() { s2CellId = cell }, new UserList() { s2CellId = cell }));

                foreach (var q in qres)
                {
                    var toremove = q.Start.list.FirstOrDefault(s => s == uid);

                    if (toremove == default(Guid))
                        return false;
                    q.Start.list.Remove(toremove);

                    var removeOutParameter = new S2CellId();
                    var onemoreOutParameter = new S2CellId();
                    _currentUsersLocations.TryRemove(toremove, out removeOutParameter);
                    _accurateUsersLocations.TryRemove(toremove, out onemoreOutParameter);

                    if (q.Start.list.Count == 0)
                    {
                        rtree.Remove(q);
                    }
                }
                return true;
            }
        }
        

        static S2Point pointFromLatLng(double lat, double lon)
        {
            var phi = ConvertToRadians(lat);
            var theta = ConvertToRadians(lon);
            var cosPhi = Math.Cos(phi);
            return new S2Point(Math.Cos(theta) * cosPhi, Math.Sin(theta) * cosPhi, Math.Sin(phi));
        }
        static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }


        const double EarthRadiusM = 6371010.0;
    }
}
