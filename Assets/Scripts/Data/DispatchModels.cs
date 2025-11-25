using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DispatchQuest.Data
{
    public enum TechnicianStatus
    {
        Available,
        Busy,
        OffShift
    }

    public enum JobPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum JobStatus
    {
        Unassigned,
        Assigned,
        InProgress,
        Completed
    }

    [Serializable]
    public class JobTicket
    {
        public string Id;
        public string Title;
        public string ClientName;
        public Vector2 MapPosition;
        public List<string> RequiredSkills = new();
        public float EstimatedDurationHours;
        public JobPriority Priority;
        public JobStatus Status = JobStatus.Unassigned;
        public Technician AssignedTechnician;

        public DateTime? ScheduledStartTime;
        public DateTime? ScheduledEndTime;
        public List<string> Notes = new();

        public void AssignToTechnician(Technician technician)
        {
            AssignedTechnician = technician;
            Status = JobStatus.Assigned;
        }
    }

    [Serializable]
    public class Technician
    {
        public string Id;
        public string Name;
        public List<string> Skills = new();
        public Vector2 MapPosition;
        public TechnicianStatus Status = TechnicianStatus.Available;
        public List<JobTicket> AssignedJobs = new();
        public List<string> Notes = new();

        public float DailyWorkloadHours => AssignedJobs.Where(j => j != null && j.Status != JobStatus.Completed)
            .Sum(j => j.EstimatedDurationHours);

        public void AssignJob(JobTicket job)
        {
            if (job == null || AssignedJobs.Contains(job)) return;
            AssignedJobs.Add(job);
            job.AssignToTechnician(this);
            if (Status != TechnicianStatus.OffShift)
            {
                Status = TechnicianStatus.Busy;
            }
        }

        public void RemoveJob(JobTicket job)
        {
            if (job == null) return;
            if (AssignedJobs.Remove(job))
            {
                job.AssignedTechnician = null;
                job.Status = JobStatus.Unassigned;
                job.ScheduledStartTime = null;
                job.ScheduledEndTime = null;
                if (AssignedJobs.Count == 0 && Status == TechnicianStatus.Busy)
                {
                    Status = TechnicianStatus.Available;
                }
            }
        }
    }
}
