using System;
using System.Collections.Generic;
using System.Linq;
using DispatchQuest.Data;
using UnityEngine;

namespace DispatchQuest.Managers
{
    public class DispatchDataManager : MonoBehaviour
    {
        [Header("Data Sets")]
        public List<Technician> Technicians = new();
        public List<JobTicket> Jobs = new();

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
    }
}
