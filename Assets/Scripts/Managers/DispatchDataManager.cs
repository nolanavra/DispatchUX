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
        [SerializeField] [Tooltip("Meters of jitter applied so overlapping cafes do not stack markers.")]
        private float cafeLocationJitterMeters = 30f;
        [SerializeField] [Tooltip("Extra padding applied around cafe bounds to keep markers on-screen.")]
        private float cafeBoundsPaddingMeters = 75f;

        [Header("Map Bounds")]
        public Vector2 MapMin = Vector2.zero;
        public Vector2 MapMax = new(100f, 100f);

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

            var hasCafeData = TryGetCafeJobCandidates(out var cafeJobs, out var cafePositions);
            if (hasCafeData)
            {
                UpdateMapBoundsFromCafes(cafePositions.Values);
            }

            int techCount = UnityEngine.Random.Range(4, 7);
            var availableIndices = Enumerable.Range(0, TechnicianNames.Length).OrderBy(_ => UnityEngine.Random.value).ToList();
            for (int i = 0; i < techCount; i++)
            {
                string name = TechnicianNames[availableIndices[i % TechnicianNames.Length]];
                Technician tech = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Skills = GenerateRandomSkills(),
                    MapPosition = GenerateRandomPosition(),
                    Status = TechnicianStatus.Available
                };
                Technicians.Add(tech);
            }

            if (hasCafeData)
            {
                CreateJobsFromCafes(cafeJobs, cafePositions);
            }
            else
            {
                int jobCount = UnityEngine.Random.Range(8, 13);
                for (int i = 0; i < jobCount; i++)
                {
                    JobTicket job = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = JobTitles[UnityEngine.Random.Range(0, JobTitles.Length)],
                        ClientName = ClientNames[UnityEngine.Random.Range(0, ClientNames.Length)],
                        MapPosition = GenerateRandomPosition(),
                        RequiredSkills = GenerateRandomSkills(),
                        EstimatedDurationHours = UnityEngine.Random.Range(1f, 6.5f),
                        Priority = (JobPriority)UnityEngine.Random.Range(0, Enum.GetValues(typeof(JobPriority)).Length),
                        Status = JobStatus.Unassigned
                    };
                    Jobs.Add(job);
                }
            }
        }

        private Vector2 GenerateRandomPosition()
        {
            return new Vector2(
                UnityEngine.Random.Range(MapMin.x, MapMax.x),
                UnityEngine.Random.Range(MapMin.y, MapMax.y));
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

        private bool TryGetCafeJobCandidates(out List<Cafe> cafes, out Dictionary<Cafe, Vector2> positions)
        {
            cafes = null;
            positions = null;

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

            double originLat = validCafes.Average(c => c.lat);
            double originLon = validCafes.Average(c => c.lon);

            positions = new Dictionary<Cafe, Vector2>();
            foreach (var cafe in validCafes)
            {
                positions[cafe] = LatLonMapper.ToLocalXY(originLat, originLon, cafe.lat, cafe.lon);
            }

            cafes = validCafes
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(Mathf.Min(maxCafeJobs, validCafes.Count))
                .ToList();

            return cafes.Count > 0 && positions.Count > 0;
        }

        private void CreateJobsFromCafes(List<Cafe> cafes, Dictionary<Cafe, Vector2> positions)
        {
            foreach (var cafe in cafes)
            {
                if (!positions.TryGetValue(cafe, out var mapPosition))
                {
                    continue;
                }

                var jitter = UnityEngine.Random.insideUnitCircle * cafeLocationJitterMeters;
                Vector2 finalPosition = mapPosition + jitter;

                JobTicket job = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"{JobTitles[UnityEngine.Random.Range(0, JobTitles.Length)]} @ {cafe.name ?? cafe.brand ?? "Cafe"}",
                    ClientName = !string.IsNullOrWhiteSpace(cafe.name) ? cafe.name : cafe.brand ?? "Cafe Client",
                    MapPosition = finalPosition,
                    RequiredSkills = GenerateRandomSkills(),
                    EstimatedDurationHours = UnityEngine.Random.Range(1f, 6.5f),
                    Priority = (JobPriority)UnityEngine.Random.Range(0, Enum.GetValues(typeof(JobPriority)).Length),
                    Status = JobStatus.Unassigned,
                    Notes = BuildCafeNotes(cafe)
                };

                Jobs.Add(job);
            }
        }

        private List<string> BuildCafeNotes(Cafe cafe)
        {
            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(cafe.address))
            {
                notes.Add(cafe.address);
            }
            if (!string.IsNullOrWhiteSpace(cafe.city))
            {
                notes.Add(cafe.city);
            }
            if (!string.IsNullOrWhiteSpace(cafe.postcode))
            {
                notes.Add(cafe.postcode);
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
