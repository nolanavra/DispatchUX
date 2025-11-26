using UnityEngine;

namespace DispatchQuest.Geo
{
    public static class LatLonMapper
    {
        private const double EarthRadiusMeters = 6371000.0;

        /// <summary>
        /// Maps latitude/longitude to a flat local XY coordinate relative to an origin.
        /// x points east, y points north.
        /// </summary>
        public static Vector2 ToLocalXY(double originLat, double originLon, double targetLat, double targetLon)
        {
            double ToRadians(double angle) => Mathf.PI * angle / 180.0f;

            var dLat = ToRadians(targetLat - originLat);
            var dLon = ToRadians(targetLon - originLon);
            var originLatRad = ToRadians(originLat);

            var x = dLon * Mathf.Cos((float)originLatRad) * EarthRadiusMeters;
            var y = dLat * EarthRadiusMeters;
            return new Vector2((float)x, (float)y);
        }
    }
}
