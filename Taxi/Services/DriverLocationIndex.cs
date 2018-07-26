using Google.Common.Geometry;
using RangeTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Services
{
    public class DriverLocationIndex
    {
        private const double EarthRadiusM = 6371010.0;

        private static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        private static S2Point pointFromLatLng(double lat, double lon)
        {
            var phi = ConvertToRadians(lat);
            var theta = ConvertToRadians(lon);
            var cosPhi = Math.Cos(phi);
            return new S2Point(Math.Cos(theta) * cosPhi, Math.Sin(theta) * cosPhi, Math.Sin(phi));
        }
        public  class SimpleRangeItem : IRangeProvider<S2CellId>
        {
            public Range<S2CellId> Range { get; set; }

            public List<Guid> Content { get; set; }
        }
        public  class SimpleRangeItemComparer : IComparer<SimpleRangeItem>
        {
            public int Compare(SimpleRangeItem x, SimpleRangeItem y)
            {
                return x.Range.CompareTo(y.Range);
            }
        }

        public RangeTreeAsync<S2CellId, SimpleRangeItem> rtree;
        
        private int _level;

        private SortedDictionary<Guid, S2CellId> _currentUsersLocations;

        public DriverLocationIndex(int level)
        {
            _level = level;
            rtree = new RangeTreeAsync<S2CellId, SimpleRangeItem>(new SimpleRangeItemComparer());
            _currentUsersLocations = new SortedDictionary<Guid, S2CellId>();
        }

        private bool RemoveUser(Guid uid)
        {
            var cell = _currentUsersLocations[uid];

            var query_res = rtree.Query(cell);

            var clone = query_res.ToList();

            foreach(var q in clone)
            {
                var toremove = q.Content.FirstOrDefault(u => u == uid);

                if (toremove == null)
                    return false;
                q.Content.Remove(toremove);
                
            }
            
            if (query_res.Count > 0)
            {
                rtree.Remove(query_res[0]);
                if (clone.Count != 0)
                {
                    rtree.Add(clone[0]);///TODO: fiiiiiiiiiiiiiixxxxxxxx 
                }
                
            }
            else return false;        
            return true;
        }

        public bool AddUser(Guid uid, double lon, double lat)
        {
            if (_currentUsersLocations.ContainsKey(uid))
            {
                // return with no result silently for now
                return false;
            }

            var lonLat = S2LatLng.FromDegrees(lat, lon);

            var cellId = S2CellId.FromLatLng(lonLat);
            
            var cellIdStorageLevel = cellId.ParentForLevel(_level);

            _currentUsersLocations[uid] = cellIdStorageLevel;

            var query_res = rtree.Query(cellIdStorageLevel);

            SimpleRangeItem rangeItem = null;

            if (query_res.Count > 0)
            {
                var users = new List<Guid>();
                foreach (var item in query_res)
                {
                    users.AddRange(item.Content);
                }

                rangeItem = new SimpleRangeItem { Range = new Range<S2CellId>(cellIdStorageLevel), Content = users };

                rtree.Remove(query_res[0]);

            }

            if (rangeItem == null)
            {
                rangeItem = new SimpleRangeItem { Range = new Range<S2CellId>(cellIdStorageLevel), Content = new List<Guid>() };
            }
            rangeItem.Content.Add(uid);

            rtree.Add(rangeItem);

            return true;
        }

        public List<Guid> Search(double lon, double lat, int radius)
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

                    var qres = rtree.Query(new Range<S2CellId>(begin, end));

                    foreach (var r in qres)
                    {
                        res.AddRange(r.Content);
                    }
                }
                else
                {
                    var qres = rtree.Query(new Range<S2CellId>(sell));
                    if (qres.Count > 0)
                    {
                        foreach (var r in qres)
                        {
                            res.AddRange(r.Content);
                        }
                    }
                }
            }
            return res;
        }

    }

}

