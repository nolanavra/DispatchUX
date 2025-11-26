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
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("Cafe JSON text was empty or null.");
                return db;
            }

            // First try MiniJSON so we can hydrate the tags dictionary coming from OSM.
            var parsed = MiniJSON.Deserialize(json);
            if (parsed != null)
            {
                if (parsed is Dictionary<string, object> root)
                {
                    // Primary expected shape
                    if (TryAddCafesFromList(root, "cafes", db))
                    {
                        db.Initialize();
                        return db;
                    }

                    // Overpass API shape (elements array)
                    if (TryAddCafesFromList(root, "elements", db))
                    {
                        db.Initialize();
                        return db;
                    }
                }
                else if (parsed is List<object> arrayRoot)
                {
                    // Allow raw array files without a wrapper object.
                    AddCafeEntries(arrayRoot, db);
                    db.Initialize();
                    return db;
                }
            }

            // Fallback: attempt JsonUtility in case MiniJSON failed unexpectedly.
            try
            {
                var wrapper = JsonUtility.FromJson<CafeDatabaseWrapper>(json);
                if (wrapper?.cafes != null && wrapper.cafes.Count > 0)
                {
                    db.cafes = wrapper.cafes;
                    db.Initialize();
                    return db;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Cafe JSON fallback parsing failed: {ex.Message}");
            }

            Debug.LogWarning("Cafe JSON did not contain a recognizable cafe array ('cafes', 'elements', or top-level array').");
            return db;
        }

        private static bool TryAddCafesFromList(Dictionary<string, object> root, string key, CafeDatabase db)
        {
            if (root.TryGetValue(key, out var listObj) && listObj is List<object> list)
            {
                AddCafeEntries(list, db);
                return db.cafes.Count > 0;
            }
            return false;
        }

        private static void AddCafeEntries(IEnumerable<object> entries, CafeDatabase db)
        {
            foreach (var entry in entries)
            {
                if (entry is Dictionary<string, object> cafeDict)
                {
                    db.cafes.Add(Cafe.FromDictionary(cafeDict));
                }
            }
        }

        [Serializable]
        private class CafeDatabaseWrapper
        {
            public List<Cafe> cafes;
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
