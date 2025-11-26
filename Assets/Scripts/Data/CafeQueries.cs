using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatchQuest.Data
{
    public static class CafeQueries
    {
        private const double EarthRadiusKm = 6371.0;

        public static IEnumerable<Cafe> GetCafesNearLatLon(IEnumerable<Cafe> source, double lat, double lon, double maxKm)
        {
            return source.Where(cafe => HaversineKm(lat, lon, cafe.lat, cafe.lon) <= maxKm);
        }

        public static IEnumerable<Cafe> GetByBrand(IEnumerable<Cafe> source, string brand)
        {
            if (string.IsNullOrWhiteSpace(brand)) return Enumerable.Empty<Cafe>();
            return source.Where(cafe => string.Equals(cafe.brand, brand, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Cafe> GetByName(IEnumerable<Cafe> source, string namePart)
        {
            if (string.IsNullOrWhiteSpace(namePart)) return Enumerable.Empty<Cafe>();
            return source.Where(cafe => !string.IsNullOrWhiteSpace(cafe.name) && cafe.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public static IEnumerable<Cafe> GetByAddress(IEnumerable<Cafe> source, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Enumerable.Empty<Cafe>();
            return source.Where(cafe => !string.IsNullOrWhiteSpace(cafe.address) && cafe.address.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public static IEnumerable<Cafe> GetByCity(IEnumerable<Cafe> source, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return Enumerable.Empty<Cafe>();
            return source.Where(cafe => !string.IsNullOrWhiteSpace(cafe.city) && cafe.city.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Cafe> GetByPostcode(IEnumerable<Cafe> source, string zip)
        {
            if (string.IsNullOrWhiteSpace(zip)) return Enumerable.Empty<Cafe>();
            return source.Where(cafe => !string.IsNullOrWhiteSpace(cafe.postcode) && cafe.postcode.Equals(zip, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<Cafe> SortByDistance(IEnumerable<Cafe> source, double lat, double lon)
        {
            return source.OrderBy(cafe => HaversineKm(lat, lon, cafe.lat, cafe.lon));
        }

        public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            double ToRadians(double angle) => Math.PI * angle / 180.0;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Pow(Math.Sin(dLon / 2), 2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadiusKm * c;
        }
    }
}
