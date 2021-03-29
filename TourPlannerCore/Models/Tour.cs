using System;

namespace TourPlanner.Core.Models
{
    public class Tour
    {
        public Guid Id { get; }

        public string Name { get; }

        public Route Route { get; }
    }
}
