using System;
using System.Collections.Generic;
using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using UnityEngine;

namespace DispatchQuest.Services
{
    /// <summary>
    /// Simple nearest-neighbor route planner that orders a technician's jobs for the day.
    /// </summary>
    public class RoutePlannerService : MonoBehaviour
    {
        [SerializeField] private DispatchDataManager dataManager;
        [SerializeField] private float workdayStartHour = 8f;

        public void GenerateDailyRoutesForAllTechnicians()
        {
            if (dataManager == null) return;
            foreach (var tech in dataManager.Technicians)
            {
                GenerateRouteForTechnician(tech);
            }

            dataManager.NotifyRoutesGenerated();
            dataManager.NotifyDataChanged();
        }

        public List<JobTicket> GenerateRouteForTechnician(Technician tech)
        {
            if (tech == null || tech.AssignedJobs.Count == 0) return new List<JobTicket>();

            var remaining = new List<JobTicket>(tech.AssignedJobs.Where(j => j.Status != JobStatus.Completed));
            List<JobTicket> ordered = new();
            Vector2 currentPos = tech.MapPosition;

            while (remaining.Count > 0)
            {
                JobTicket next = null;
                float closest = float.MaxValue;
                foreach (var job in remaining)
                {
                    float dist = Vector2.Distance(currentPos, job.MapPosition);
                    if (dist < closest)
                    {
                        closest = dist;
                        next = job;
                    }
                }

                if (next == null) break;
                ordered.Add(next);
                remaining.Remove(next);
                currentPos = next.MapPosition;
            }

            // Update assignment order and simple schedule
            tech.AssignedJobs = ordered;
            DateTime currentTime = DateTime.Today.AddHours(workdayStartHour);
            foreach (var job in tech.AssignedJobs)
            {
                job.ScheduledStartTime = currentTime;
                job.ScheduledEndTime = currentTime.AddHours(job.EstimatedDurationHours);
                currentTime = job.ScheduledEndTime.Value;
            }

            return ordered;
        }
    }
}
