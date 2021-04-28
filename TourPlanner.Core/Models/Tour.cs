using System;
using System.Collections.Generic;

namespace TourPlanner.Core.Models
{
    /// <summary>
    /// Contains all relevant information about a tour.
    /// </summary>
    public class Tour
    {
        /// <summary>
        /// A unique ID for identifying the tour.
        /// </summary>
        public Guid TourId { get; init; }

        /// <summary>
        /// A user-decided name for the tour.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// File path to the preview image.
        /// </summary>
        public string ImagePath { get; init; }

        /// <summary>
        /// A user-decided description for the tour.
        /// </summary>
        public string CustomDescription { get; set; }

        /// <summary>
        /// Reference to a <see cref="Models.Route"/> object containting
        /// further information specifically regarding the route.
        /// </summary>
        public Route Route { get; init; }

        /// <summary>
        /// Contains the log entries for this tour.
        /// </summary>
        public ICollection<LogEntry> Log { get; init; }
    }
}
