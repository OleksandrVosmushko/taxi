using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geolocation;


namespace Taxi.Helpers
{
    public static class Location
    {
        const double EarthRadiusM = 6371010.0;

        public static double CalculateKilometersDistance(double lat1, double lon1, double lat2, double lon2)
        {
            return GeoCalculator.GetDistance(lat1, lon1, lat2, lon2, 5, DistanceUnit.Kilometers);
        }
        public static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
        
    }
}
