using System.Collections.Generic;
using System.Linq;
using DispatchQuest.Data;
using DispatchQuest.Managers;
using UnityEngine;

namespace DispatchQuest.Services
{
    /// <summary>
    /// Lightweight scoring helper to recommend technicians for a job based on skills and proximity.
    /// </summary>
    public class JobRecommendationService : MonoBehaviour
    {
        [SerializeField] private DispatchDataManager dataManager;
        [SerializeField] private float skillWeight = 2f;
        [SerializeField] private float distanceWeight = 1f;

        public List<Technician> GetRecommendedTechnicians(JobTicket job, int maxCount = 3)
        {
            if (job == null || dataManager == null || dataManager.Technicians == null)
            {
                return new List<Technician>();
            }

            var candidates = dataManager.Technicians.Where(t => t.Status != TechnicianStatus.OffShift).ToList();

            var scored = new List<(Technician tech, float score)>();
            foreach (var tech in candidates)
            {
                int skillMatches = job.RequiredSkills.Count > 0
                    ? tech.Skills.Intersect(job.RequiredSkills).Count()
                    : 0;
                float distance = Vector2.Distance(tech.MapPosition, job.MapPosition);
                float distanceScore = distance > 0.01f ? 1f / distance : 5f; // closer is better

                float score = (skillMatches * skillWeight) + (distanceScore * distanceWeight);
                scored.Add((tech, score));
            }

            return scored
                .OrderByDescending(s => s.score)
                .ThenBy(s => s.tech.DailyWorkloadHours)
                .Take(maxCount)
                .Select(s => s.tech)
                .ToList();
        }
    }
}
