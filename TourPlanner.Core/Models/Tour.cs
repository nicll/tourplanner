using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TourPlanner.Core.Internal;

namespace TourPlanner.Core.Models
{
    /// <summary>
    /// Contains all relevant information about a tour.
    /// </summary>
    public class Tour : IChangeTracking
    {
        private readonly ChangeTracker _tracker = new();
        private readonly ChangeTrackingCollection<LogEntry> _log;
        private string _name, _description;

        /// <summary>
        /// A unique ID for identifying the tour.
        /// </summary>
        public Guid TourId { get; init; }

        /// <summary>
        /// A user-decided name for the tour.
        /// </summary>
        [MaxLength(64)]
        public string Name
        {
            get => _name;
            set => _tracker.SetProperty(ref _name, value);
        }

        /// <summary>
        /// File path to the preview image.
        /// </summary>
        public string ImagePath { get; init; }

        /// <summary>
        /// A user-decided description for the tour.
        /// </summary>
        [MaxLength(2048)]
        public string CustomDescription
        {
            get => _description;
            set => _tracker.SetProperty(ref _description, value);
        }

        /// <summary>
        /// Reference to a <see cref="Models.Route"/> object containting
        /// further information specifically regarding the route.
        /// </summary>
        public Route Route { get; init; }

        /// <summary>
        /// Contains the log entries for this tour.
        /// </summary>
        public ICollection<LogEntry> Log => _log;

        public bool IsChanged => _tracker.IsChanged;

        public void AcceptChanges()
        {
            _tracker.AcceptChanges();
            _log.AcceptChanges();
        }

        public Tour()
            => _log = new ChangeTrackingCollection<LogEntry>();

        public Tour(ICollection<LogEntry> logEntries)
            => _log = new ChangeTrackingCollection<LogEntry>(logEntries);
    }
}
