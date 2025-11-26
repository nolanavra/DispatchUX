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
    public class SkillProficiencyEntry
    {
        public string skillName;
        public float proficiency;
    }

    [Serializable]
    public class RequiredSkillToggle
    {
        public string skillName;
        public bool isRequired;
    }

    [Serializable]
    public class JobNote
    {
        public string author;
        public string technicianId;
        public string text;
        public DateTime timestamp = DateTime.Now;
    }

    [Serializable]
    public class JobTicket
    {
        public string Id;
        public string Title;
        public string ClientName;
        public Vector2 MapPosition;
        public double Latitude;
        public double Longitude;
        public List<string> RequiredSkills = new();
        public float EstimatedDurationHours;
        public JobPriority Priority;
        public JobStatus Status = JobStatus.Unassigned;
        public Technician AssignedTechnician;

        public DateTime? ScheduledStartTime;
        public DateTime? ScheduledEndTime;

        public string Address;
        public string DetailedDescription;
        public List<RequiredSkillToggle> RequiredSkillToggles = new();
        public List<JobNote> Notes = new();

        public void AssignToTechnician(Technician technician)
        {
            AssignedTechnician = technician;
            Status = JobStatus.Assigned;
        }

        public void EnsureRequiredSkillToggles()
        {
            if (RequiredSkillToggles == null)
            {
                RequiredSkillToggles = new List<RequiredSkillToggle>();
            }

            if (RequiredSkillToggles.Count == 0 && RequiredSkills != null)
            {
                foreach (var skill in RequiredSkills)
                {
                    RequiredSkillToggles.Add(new RequiredSkillToggle
                    {
                        skillName = skill,
                        isRequired = true
                    });
                }
            }
        }

        public void SyncRequiredSkillsFromToggles()
        {
            EnsureRequiredSkillToggles();
            RequiredSkills ??= new List<string>();
            RequiredSkills.Clear();
            foreach (var toggle in RequiredSkillToggles)
            {
                if (toggle != null && toggle.isRequired && !string.IsNullOrWhiteSpace(toggle.skillName))
                {
                    RequiredSkills.Add(toggle.skillName);
                }
            }
        }

        public IEnumerable<JobNote> GetRelevantNotes()
        {
            return Notes ?? Enumerable.Empty<JobNote>();
        }
    }

    [Serializable]
    public class Technician
    {
        public string Id;
        public string Name;
        public List<string> Skills = new();
        public Vector2 MapPosition;
        public double Latitude;
        public double Longitude;
        public TechnicianStatus Status = TechnicianStatus.Available;
        public List<JobTicket> AssignedJobs = new();
        public List<JobNote> Notes = new();

        public string Address;
        public string Phone;
        public float PlannedHoursToday;
        public float ExpectedHoursToday;
        public List<SkillProficiencyEntry> SkillProficiencyEntries = new();

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

        public float GetSkillProficiency(string skillName, float defaultValue = 0f)
        {
            if (string.IsNullOrWhiteSpace(skillName) || SkillProficiencyEntries == null)
            {
                return defaultValue;
            }

            var match = SkillProficiencyEntries.FirstOrDefault(s => string.Equals(s.skillName, skillName, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.proficiency : defaultValue;
        }

        public void EnsureSkillProficiencyEntries()
        {
            if (SkillProficiencyEntries == null)
            {
                SkillProficiencyEntries = new List<SkillProficiencyEntry>();
            }

            if (Skills == null)
            {
                Skills = new List<string>();
            }

            foreach (var skill in Skills)
            {
                if (SkillProficiencyEntries.All(e => !string.Equals(e.skillName, skill, StringComparison.OrdinalIgnoreCase)))
                {
                    SkillProficiencyEntries.Add(new SkillProficiencyEntry
                    {
                        skillName = skill,
                        proficiency = 1f
                    });
                }
            }
        }
    }
}
