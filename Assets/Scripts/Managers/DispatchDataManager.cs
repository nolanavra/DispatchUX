using System;
using System.Collections.Generic;
using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Geo;
using UnityEngine;

namespace DispatchQuest.Managers
{
    public class DispatchDataManager : MonoBehaviour
    {
        [Header("Data Sets")]
        public List<Technician> Technicians = new();
        public List<JobTicket> Jobs = new();

        [Header("Cafe Sourcing")]
        [SerializeField] private CafeDatabaseLoader cafeLoader;
        [SerializeField] [Tooltip("Maximum number of jobs to pull from the cafe dataset.")]
        private int maxCafeJobs = 12;
        [SerializeField] [Tooltip("Extra padding applied around cafe bounds to keep markers on-screen.")]
        private float cafeBoundsPaddingMeters = 75f;

        [Header("Map Bounds")]
        public Vector2 MapMin = Vector2.zero;
        public Vector2 MapMax = new(100f, 100f);

        [Header("Map Projection")]
        [SerializeField] private double fallbackOriginLatitude = 42.8864; // Buffalo-ish default
        [SerializeField] private double fallbackOriginLongitude = -78.8784;
        public double MapOriginLatitude { get; private set; }
        public double MapOriginLongitude { get; private set; }

        private Dictionary<Cafe, Vector2> cafePositions = new();
        public IReadOnlyDictionary<Cafe, Vector2> CafePositions => cafePositions;
        public List<Cafe> CafeSites { get; private set; } = new();

        [Header("Workload Thresholds (hours)")]
        [SerializeField] private float workloadLowThreshold = 4f;
        [SerializeField] private float workloadHighThreshold = 8f;

        public event Action<JobTicket, Technician> OnJobAssigned;
        public event Action OnDataChanged;
        public event Action OnWorkloadChanged;
        public event Action OnRoutesGenerated;

        private static readonly string[] TechnicianNames =
        {
            "Avery Shields", "Morgan Trent", "Jamie Rivera", "Casey Lin", "Dakota Hale", "Skylar Owens"
        };

        private static readonly string[] TechnicianAddresses =
        {
            "123 Harbor Ave", "42 Industrial Way", "88 Grove Street", "15 Skylark Blvd", "9 Summit Road", "210 Riverside Dr"
        };

        private static readonly string[] TechnicianPhones =
        {
            "555-0101", "555-0102", "555-0103", "555-0104", "555-0105", "555-0106"
        };

        private static readonly string[] ClientNames =
        {
            "Northwind", "Acme Corp", "Blue Harbor", "Evergreen", "Redline Logistics", "FutureTech"
        };

        private static readonly string[] JobTitles =
        {
            "Server Maintenance", "HVAC Tune-Up", "Replace Pump", "Network Audit", "Panel Inspection", "Outlet Install",
            "Firmware Update", "Pipe Reroute"
        };

        private static readonly string[] Skills = { "Electrical", "HVAC", "Plumbing", "IT" };

        public float WorkloadLowThreshold => workloadLowThreshold;
        public float WorkloadHighThreshold => workloadHighThreshold;

        private void Awake()
        {
            GenerateDummyData();
            OnDataChanged?.Invoke();
            OnWorkloadChanged?.Invoke();
        }

        public void GenerateDummyData()
        {
            Technicians.Clear();
            Jobs.Clear();

            CafeSites = new List<Cafe>();
            cafePositions = new Dictionary<Cafe, Vector2>();

            MapOriginLatitude = fallbackOriginLatitude;
            MapOriginLongitude = fallbackOriginLongitude;
            MapMin = Vector2.zero;
            MapMax = new Vector2(100f, 100f);

            var hasCafeData = TryLoadCafePositions(out var allCafes, out var positions, out var originLat, out var originLon);
            if (hasCafeData)
            {
                CafeSites = allCafes;
                cafePositions = positions;
                MapOriginLatitude = originLat;
                MapOriginLongitude = originLon;
                UpdateMapBoundsFromCafes(positions.Values);
                CreateTechniciansFromSites(allCafes, positions);
                CreateJobsFromCafes(allCafes, positions);
            }
            else
            {
                CreateRandomTechnicians();
                CreateRandomJobs();
            }
        }

        private Vector2 GenerateRandomPosition()
        {
            return new Vector2(
                UnityEngine.Random.Range(MapMin.x, MapMax.x),
                UnityEngine.Random.Range(MapMin.y, MapMax.y));
        }

        private (double lat, double lon) LocalToLatLon(Vector2 mapPosition)
        {
            const double earthRadiusMeters = 6371000.0;
            double originLatRad = MapOriginLatitude * Math.PI / 180.0;
            double lat = MapOriginLatitude + (mapPosition.y / earthRadiusMeters) * (180.0 / Math.PI);
            double lon = MapOriginLongitude + (mapPosition.x / (earthRadiusMeters * Math.Cos(originLatRad))) * (180.0 / Math.PI);
            return (lat, lon);
        }

        private List<string> GenerateRandomSkills()
        {
            List<string> list = new();
            int skillCount = UnityEngine.Random.Range(1, Skills.Length + 1);
            var shuffled = Skills.OrderBy(_ => UnityEngine.Random.value).ToList();
            for (int i = 0; i < skillCount; i++)
            {
                list.Add(shuffled[i]);
            }
            return list;
        }

        private void CreateTechniciansFromSites(List<Cafe> cafes, Dictionary<Cafe, Vector2> positions)
        {
            if (cafes == null || positions == null || cafes.Count == 0) return;

            int techCount = Mathf.Clamp(UnityEngine.Random.Range(4, 7), 1, cafes.Count);
            var availableIndices = Enumerable.Range(0, TechnicianNames.Length).OrderBy(_ => UnityEngine.Random.value).ToList();
            var shuffledSites = cafes.OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int i = 0; i < techCount && i < shuffledSites.Count; i++)
            {
                var cafe = shuffledSites[i];
                if (!positions.TryGetValue(cafe, out var mapPos)) continue;

                Technician tech = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = TechnicianNames[availableIndices[i % TechnicianNames.Length]],
                    Skills = GenerateRandomSkills(),
                    MapPosition = mapPos,
                    Latitude = cafe.lat,
                    Longitude = cafe.lon,
                    Status = TechnicianStatus.Available,
                    Address = string.IsNullOrWhiteSpace(cafe.address) ? cafe.ComposeAddress() : cafe.address,
                    Phone = TechnicianPhones[UnityEngine.Random.Range(0, TechnicianPhones.Length)],
                    PlannedHoursToday = UnityEngine.Random.Range(3f, 9f),
                    ExpectedHoursToday = UnityEngine.Random.Range(6f, 9f)
                };
                tech.EnsureSkillProficiencyEntries();
                Technicians.Add(tech);
            }
        }

        private void CreateRandomTechnicians()
        {
            int techCount = UnityEngine.Random.Range(4, 7);
            var availableIndices = Enumerable.Range(0, TechnicianNames.Length).OrderBy(_ => UnityEngine.Random.value).ToList();

            for (int i = 0; i < techCount; i++)
            {
                var mapPos = GenerateRandomPosition();
                var (lat, lon) = LocalToLatLon(mapPos);

                Technician tech = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = TechnicianNames[availableIndices[i % TechnicianNames.Length]],
                    Skills = GenerateRandomSkills(),
                    MapPosition = mapPos,
                    Latitude = lat,
                    Longitude = lon,
                    Status = TechnicianStatus.Available,
                    Address = TechnicianAddresses[UnityEngine.Random.Range(0, TechnicianAddresses.Length)],
                    Phone = TechnicianPhones[UnityEngine.Random.Range(0, TechnicianPhones.Length)],
                    PlannedHoursToday = UnityEngine.Random.Range(3f, 9f),
                    ExpectedHoursToday = UnityEngine.Random.Range(6f, 9f)
                };
                tech.EnsureSkillProficiencyEntries();
                Technicians.Add(tech);
            }
        }

        private void CreateRandomJobs()
        {
            int jobCount = UnityEngine.Random.Range(8, 13);
            for (int i = 0; i < jobCount; i++)
            {
                var mapPos = GenerateRandomPosition();
                var (lat, lon) = LocalToLatLon(mapPos);

                JobTicket job = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = JobTitles[UnityEngine.Random.Range(0, JobTitles.Length)],
                    ClientName = ClientNames[UnityEngine.Random.Range(0, ClientNames.Length)],
                    MapPosition = mapPos,
                    Latitude = lat,
                    Longitude = lon,
                    RequiredSkills = GenerateRandomSkills(),
                    EstimatedDurationHours = UnityEngine.Random.Range(1f, 6.5f),
                    Priority = (JobPriority)UnityEngine.Random.Range(0, Enum.GetValues(typeof(JobPriority)).Length),
                    Status = JobStatus.Unassigned,
                    Address = $"{UnityEngine.Random.Range(10, 999)} Dispatch Lane",
                    DetailedDescription = "Auto-generated task description for testing."
                };
                job.EnsureRequiredSkillToggles();
                Jobs.Add(job);
            }
        }

        private bool TryLoadCafePositions(out List<Cafe> cafes, out Dictionary<Cafe, Vector2> positions, out double originLat, out double originLon)
        {
            cafes = null;
            positions = null;
            originLat = MapOriginLatitude;
            originLon = MapOriginLongitude;

            var db = CafeDatabaseLoader.Database ?? CafeDatabaseLoader.LoadFromResources();
            if (db?.cafes == null || db.cafes.Count == 0)
            {
                return false;
            }

            var validCafes = db.cafes
                .Where(c => !double.IsNaN(c.lat) && !double.IsNaN(c.lon))
                .ToList();

            if (validCafes.Count == 0)
            {
                return false;
            }

            originLat = validCafes.Average(c => c.lat);
            originLon = validCafes.Average(c => c.lon);

            positions = new Dictionary<Cafe, Vector2>();
            foreach (var cafe in validCafes)
            {
                positions[cafe] = LatLonMapper.ToLocalXY(originLat, originLon, cafe.lat, cafe.lon);
            }

            cafes = validCafes;
            return cafes.Count > 0 && positions.Count > 0;
        }

        private void CreateJobsFromCafes(List<Cafe> cafes, Dictionary<Cafe, Vector2> positions)
        {
            var jobCandidates = cafes
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(Mathf.Min(maxCafeJobs, cafes.Count))
                .ToList();

            foreach (var cafe in jobCandidates)
            {
                if (!positions.TryGetValue(cafe, out var mapPosition))
                {
                    continue;
                }

                var amenity = cafe.TagLookup != null && cafe.TagLookup.TryGetValue("amenity", out var amenityValue)
                    ? amenityValue
                    : null;

                var amenity = cafe.TagLookup != null && cafe.TagLookup.TryGetValue("amenity", out var amenityValue)
                    ? amenityValue
                    : null;

                JobTicket job = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"{JobTitles[UnityEngine.Random.Range(0, JobTitles.Length)]} @ {cafe.name ?? cafe.brand ?? "Cafe"}",
                    ClientName = !string.IsNullOrWhiteSpace(cafe.name) ? cafe.name : cafe.brand ?? "Cafe Client",
                    MapPosition = mapPosition,
                    Latitude = cafe.lat,
                    Longitude = cafe.lon,
                    RequiredSkills = GenerateRandomSkills(),
                    EstimatedDurationHours = UnityEngine.Random.Range(1f, 6.5f),
                    Priority = (JobPriority)UnityEngine.Random.Range(0, Enum.GetValues(typeof(JobPriority)).Length),
                    Status = JobStatus.Unassigned,
                    Address = string.IsNullOrWhiteSpace(cafe.address) ? cafe.ComposeAddress() : cafe.address,
                    DetailedDescription = !string.IsNullOrWhiteSpace(amenity)
                        ? amenity
                        : cafe.name ?? cafe.brand ?? "Maintenance Task",
                    Notes = BuildCafeNotes(cafe)
                };

                job.EnsureRequiredSkillToggles();
                Jobs.Add(job);
            }
        }

        public void AssignJobToTechnician(JobTicket job, Technician technician)
        {
            if (job == null || technician == null || job.Status == JobStatus.Completed) return;
            if (job.AssignedTechnician == technician) return;

            // If job is currently assigned, remove from previous technician
            if (job.AssignedTechnician != null)
            {
                job.AssignedTechnician.RemoveJob(job);
            }

            technician.AssignJob(job);
            OnJobAssigned?.Invoke(job, technician);
            NotifyDataChanged();
            OnWorkloadChanged?.Invoke();
        }

        public float GetTechnicianWorkloadHours(Technician tech)
        {
            if (tech == null) return 0f;
            return tech.AssignedJobs.Where(j => j != null && j.Status != JobStatus.Completed)
                .Sum(j => j.EstimatedDurationHours);
        }

        public List<JobTicket> GetUnassignedJobs()
        {
            return Jobs.Where(j => j.Status == JobStatus.Unassigned).ToList();
        }

        public void NotifyRoutesGenerated()
        {
            OnRoutesGenerated?.Invoke();
            OnWorkloadChanged?.Invoke();
        }

        public void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        private List<JobNote> BuildCafeNotes(Cafe cafe)
        {
            var notes = new List<JobNote>();
            if (!string.IsNullOrWhiteSpace(cafe.address))
            {
                notes.Add(new JobNote { author = "Data Import", text = cafe.address, timestamp = DateTime.Now });
            }
            if (!string.IsNullOrWhiteSpace(cafe.city))
            {
                notes.Add(new JobNote { author = "Data Import", text = cafe.city, timestamp = DateTime.Now });
            }
            if (!string.IsNullOrWhiteSpace(cafe.postcode))
            {
                notes.Add(new JobNote { author = "Data Import", text = cafe.postcode, timestamp = DateTime.Now });
            }
            return notes;
        }

        private void UpdateMapBoundsFromCafes(IEnumerable<Vector2> positions)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var pos in positions)
            {
                if (pos.x < minX) minX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y > maxY) maxY = pos.y;
            }

            if (minX == float.MaxValue || minY == float.MaxValue)
            {
                return;
            }

            MapMin = new Vector2(minX - cafeBoundsPaddingMeters, minY - cafeBoundsPaddingMeters);
            MapMax = new Vector2(maxX + cafeBoundsPaddingMeters, maxY + cafeBoundsPaddingMeters);
        }
    }
}
