using System;
using System.Collections.Generic;
using System.Linq;

namespace TourPlanner.Core.Models
{
    /// <summary>
    /// Contains information about the chosen pathway for a tour.
    /// </summary>
    public record Route
    {
        private double? _totalDistance;

        /// <summary>
        /// ID used for uniquely identifying the route.
        /// Specified by the data provider.
        /// </summary>
        public string RouteId { get; init; }

        /// <summary>
        /// Total distance of all steps.
        /// May be manually overridden at initialization.
        /// </summary>
        public double TotalDistance
        {
            get => _totalDistance ??= Steps.Sum(step => step.Distance);
            init => _totalDistance = value;
        }

        /// <summary>
        /// The address of the starting point.
        /// </summary>
        public string StartLocation { get; init; }

        /// <summary>
        /// The address of the ending point.
        /// </summary>
        public string EndLocation { get; init; }

        /// <summary>
        /// A list of the steps that the route is made up of.
        /// </summary>
        public IReadOnlyList<Step> Steps { get; init; }
    }
}
