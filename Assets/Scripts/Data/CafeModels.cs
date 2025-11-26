using System;
using System.Collections.Generic;
using UnityEngine;

namespace DispatchQuest.Data
{
    [Serializable]
    public class CafeDatabase
    {
        public List<Cafe> cafes = new();

        public void Initialize()
        {
            if (cafes == null)
            {
                cafes = new List<Cafe>();
            }

            foreach (var cafe in cafes)
            {
                cafe?.InitializeTags();
            }
        }

        public static CafeDatabase FromJson(string json)
        {
            var db = new CafeDatabase();
            var root = MiniJSON.Deserialize(json) as Dictionary<string, object>;
            if (root == null || !root.TryGetValue("cafes", out var cafesObj))
            {
                Debug.LogWarning("Cafe JSON did not contain a 'cafes' array.");
                return db;
            }

            if (cafesObj is List<object> list)
            {
                foreach (var entry in list)
                {
                    if (entry is Dictionary<string, object> cafeDict)
                    {
                        db.cafes.Add(Cafe.FromDictionary(cafeDict));
                    }
                }
            }
            db.Initialize();
            return db;
        }
    }

    [Serializable]
    public class Cafe
    {
        public string id;
        public string name;
        public string brand;
        public string branch;
        public string address;
        public string city;
        public string postcode;
        public double lat;
        public double lon;
        public List<SerializableTag> tags = new();

        [NonSerialized]
        private Dictionary<string, string> tagLookup;

        public Dictionary<string, string> TagLookup
        {
            get
            {
                if (tagLookup == null)
                {
                    tagLookup = BuildTagLookup();
                }
                return tagLookup;
            }
        }

        public static Cafe FromDictionary(Dictionary<string, object> source)
        {
            var cafe = new Cafe
            {
                id = GetString(source, "id"),
                name = GetString(source, "name"),
                brand = GetString(source, "brand"),
                branch = GetString(source, "branch"),
                address = GetString(source, "address"),
                city = GetString(source, "city"),
                postcode = GetString(source, "postcode"),
                lat = GetDouble(source, "lat"),
                lon = GetDouble(source, "lon")
            };

            if (source.TryGetValue("tags", out var tagsObj) && tagsObj is Dictionary<string, object> tagDict)
            {
                foreach (var kvp in tagDict)
                {
                    cafe.tags.Add(new SerializableTag(kvp.Key, kvp.Value?.ToString()));
                }
            }

            cafe.InitializeTags();
            return cafe;
        }

        public void InitializeTags()
        {
            tagLookup = BuildTagLookup();
            if (string.IsNullOrWhiteSpace(address))
            {
                address = ComposeAddress();
            }
        }

        public string ComposeAddress()
        {
            var parts = new List<string>();
            if (TagLookup.TryGetValue("addr:housenumber", out var number) && !string.IsNullOrWhiteSpace(number))
            {
                parts.Add(number.Trim());
            }
            if (TagLookup.TryGetValue("addr:street", out var street) && !string.IsNullOrWhiteSpace(street))
            {
                parts.Add(street.Trim());
            }
            var addressLine = string.Join(" ", parts);
            var localityParts = new List<string>();
            if (TagLookup.TryGetValue("addr:city", out var addrCity) && !string.IsNullOrWhiteSpace(addrCity))
            {
                localityParts.Add(addrCity.Trim());
                city = city ?? addrCity.Trim();
            }
            if (TagLookup.TryGetValue("addr:postcode", out var zip) && !string.IsNullOrWhiteSpace(zip))
            {
                localityParts.Add(zip.Trim());
                postcode = postcode ?? zip.Trim();
            }

            if (!string.IsNullOrWhiteSpace(addressLine) || localityParts.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(addressLine) && localityParts.Count > 0)
                {
                    return $"{addressLine}, {string.Join(", ", localityParts)}";
                }
                return !string.IsNullOrWhiteSpace(addressLine) ? addressLine : string.Join(", ", localityParts);
            }

            return address;
        }

        private Dictionary<string, string> BuildTagLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag.Key))
                    {
                        lookup[tag.Key] = tag.Value ?? string.Empty;
                    }
                }
            }
            return lookup;
        }

        private static string GetString(Dictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value?.ToString() : null;
        }

        private static double GetDouble(Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is double d) return d;
                if (value is long l) return l;
                if (double.TryParse(value?.ToString(), out var parsed)) return parsed;
            }
            return 0d;
        }
    }

    [Serializable]
    public class SerializableTag
    {
        public string Key;
        public string Value;

        public SerializableTag(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
