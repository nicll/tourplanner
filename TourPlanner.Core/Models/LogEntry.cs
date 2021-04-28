using System;

namespace TourPlanner.Core.Models
{
    /// <summary>
    /// A log entry describes the times when a specific tour was performed.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Date and time when the log entry was performed.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The travelled distance.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// The time spent travelling.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The user's rating of the tour.
        /// </summary>
        public float Rating { get; set; }
    }
}
