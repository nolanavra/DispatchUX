using System;
using UnityEngine;

namespace DispatchQuest.MapSDK
{
    public static class WebMercatorUtils
    {
        private const double MinLatitude = -85.05112878;
        private const double MaxLatitude = 85.05112878;
        private const double MinLongitude = -180d;
        private const double MaxLongitude = 180d;

        public static void LatLonToTileXY(double latDeg, double lonDeg, int zoom, out int tileX, out int tileY)
        {
            latDeg = Math.Clamp(latDeg, MinLatitude, MaxLatitude);
            lonDeg = Math.Clamp(lonDeg, MinLongitude, MaxLongitude);

            double latRad = latDeg * Math.PI / 180d;
            double n = Math.Pow(2d, zoom);
            tileX = (int)Math.Floor((lonDeg + 180d) / 360d * n);
            double mercatorY = Math.Log(Math.Tan(Math.PI / 4d + latRad / 2d));
            tileY = (int)Math.Floor((1d - mercatorY / Math.PI) / 2d * n);
        }

        public static Vector2 TileXYToCenterLatLon(int tileX, int tileY, int zoom)
        {
            double n = Math.Pow(2d, zoom);
            double lonDeg = tileX / n * 360d - 180d;
            double latRad = Math.Atan(Math.Sinh(Math.PI * (1d - 2d * tileY / n)));
            double latDeg = latRad * 180d / Math.PI;
            return new Vector2((float)latDeg, (float)lonDeg);
        }

        public static Vector2 LatLonToWorldPosition(double latDeg, double lonDeg, double centerLat, double centerLon, int zoom, float worldUnitsPerTile)
        {
            LatLonToTileXY(centerLat, centerLon, zoom, out int centerX, out int centerY);
            LatLonToTileXY(latDeg, lonDeg, zoom, out int tileX, out int tileY);

            int dx = tileX - centerX;
            int dy = centerY - tileY;
            return new Vector2(dx * worldUnitsPerTile, dy * worldUnitsPerTile);
        }

        public static Vector2 WorldPositionToLatLon(Vector2 worldPos, double centerLat, double centerLon, int zoom, float worldUnitsPerTile)
        {
            LatLonToTileXY(centerLat, centerLon, zoom, out int centerX, out int centerY);

            double offsetTilesX = worldPos.x / worldUnitsPerTile;
            double offsetTilesY = worldPos.y / worldUnitsPerTile;

            double targetTileX = centerX + offsetTilesX;
            double targetTileY = centerY - offsetTilesY;

            return TileCoordsToLatLon(targetTileX, targetTileY, zoom);
        }

        private static Vector2 TileCoordsToLatLon(double tileX, double tileY, int zoom)
        {
            double n = Math.Pow(2d, zoom);
            double lonDeg = tileX / n * 360d - 180d;
            double latRad = Math.Atan(Math.Sinh(Math.PI * (1d - 2d * tileY / n)));
            double latDeg = latRad * 180d / Math.PI;
            return new Vector2((float)latDeg, (float)lonDeg);
        }
    }
}
