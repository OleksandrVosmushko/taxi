using Google.Common.Geometry;
using IntervalTree;
using RangeTree;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Taxi.Services
{
    class DriverLocationIndex
    {
        private int _level;
        private IntervalTree<UserList> rtree;
        private ConcurrentDictionary<Guid, S2CellId> _currentUsersLocations;
        static object locker = new object();

        public struct UserList : IComparable<UserList>
        {
            public S2CellId s2CellId;

            public List<Guid> list;

            public int CompareTo(UserList other)
            {
                return s2CellId.CompareTo(other.s2CellId);
            }
        }

        public DriverLocationIndex(int level)
        {
            rtree = new IntervalTree<UserList>();
            _level = level;
            _currentUsersLocations = new ConcurrentDictionary<Guid, S2CellId>();
        }

        public bool UpdateUser(Guid uid, double lon, double lat)
        {
            lock (locker)
            {
                var lonLat = S2LatLng.FromDegrees(lat, lon);

                var cellId = S2CellId.FromLatLng(lonLat);

                var cellIdStorageLevel = cellId.ParentForLevel(_level);

                if (!_currentUsersLocations.ContainsKey(uid))
                    return false;

                var oldCell = _currentUsersLocations[uid];

                if (oldCell == cellIdStorageLevel)
                {
                    return true;
                }
                _currentUsersLocations[uid] = cellIdStorageLevel;

                var query_res = rtree.Search(new UserList() { s2CellId = oldCell });

                var users = new List<Guid>();
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

                query_res = rtree.Search(new UserList() { s2CellId = cellIdStorageLevel });

                if (query_res.Count > 0)
                {
                    foreach (var item in query_res)
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

        public void AddUser(Guid uid, double lon, double lat)
        {
            lock (locker)
            {
                var lonLat = S2LatLng.FromDegrees(lat, lon);

                var cellId = S2CellId.FromLatLng(lonLat);

                var cellIdStorageLevel = cellId.ParentForLevel(_level);

                _currentUsersLocations[uid] = cellIdStorageLevel;

                var query_res = rtree.Search(new UserList() { s2CellId = cellIdStorageLevel });

                var users = new List<Guid>();
                if (query_res.Count > 0)
                {
                    foreach (var item in query_res)
                    {
                        item.Start.list.Add(uid);
                    }
                    return;
                }
                users.Add(uid);

                var toinsert = new UserList() { s2CellId = cellIdStorageLevel, list = users };

                rtree.Add(new Interval<UserList>() { Start = toinsert, End = toinsert });
            }
        }
        

        public List<Guid> Search(double lon, double lat, int radius)
        {
            lock (locker)
            {
                var latlng = S2LatLng.FromDegrees(lat, lon);

                var centerPoint = pointFromLatLng(lat, lon);

                var centerAngle = ((double)radius) / EarthRadiusM;

                var cap = S2Cap.FromAxisAngle(centerPoint, S1Angle.FromRadians(centerAngle));

                var regionCoverer = new S2RegionCoverer();

                regionCoverer.MaxLevel = 13;

                //  regionCoverer.MinLevel = 13;


                //regionCoverer.MaxCells = 1000;
                // regionCoverer.LevelMod = 0;


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


        public bool RemoveUser(Guid uid)
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

                    _currentUsersLocations.TryRemove(toremove, out removeOutParameter);

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

