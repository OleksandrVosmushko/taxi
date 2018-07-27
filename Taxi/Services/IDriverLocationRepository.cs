using Google.Common.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Services
{
    public interface IDriverLocationRepository
    {
        bool UpdateUser(Guid uid, double lon, double lat);

        bool AddUser(Guid uid, double lon, double lat);

        List<Guid> Search(double lon, double lat, int radiusMeters);

        bool RemoveUser(Guid uid);

        S2LatLng GetDriverLocation(Guid driverId); 
    }
}
