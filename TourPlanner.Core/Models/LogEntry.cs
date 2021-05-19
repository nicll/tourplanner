using System;
using System.ComponentModel;
using TourPlanner.Core.Internal;

namespace TourPlanner.Core.Models
{
    /// <summary>
    /// A log entry describes the times when a specific tour was performed.
    /// </summary>
    public class LogEntry : IChangeTracking
    {
        private readonly ChangeTracker _tracker = new();
        private DateTime _date;
        private double _distance;
        private TimeSpan _duration;
        private float _rating;

        /// <summary>
        /// A unique ID for identifying the log entry.
        /// </summary>
        public Guid LogId { get; init; }

        /// <summary>
        /// Date and time when the log entry was performed.
        /// </summary>
        public DateTime Date
        {
            get => _date;
            set => _tracker.SetProperty(ref _date, value);
        }

        /// <summary>
        /// The travelled distance.
        /// </summary>
        public double Distance
        {
            get => _distance;
            set => _tracker.SetProperty(ref _distance, value);
        }

        /// <summary>
        /// The time spent travelling.
        /// </summary>
        public TimeSpan Duration
        {
            get => _duration;
            set => _tracker.SetProperty(ref _duration, value);
        }

        /// <summary>
        /// The user's rating of the tour.
        /// </summary>
        public float Rating
        {
            get => _rating;
            set => _tracker.SetProperty(ref _rating, value);
        }

        public bool IsChanged => _tracker.IsChanged;

        public void AcceptChanges() => _tracker.AcceptChanges();
    }
}
