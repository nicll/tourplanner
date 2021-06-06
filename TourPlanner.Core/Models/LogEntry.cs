using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        private double _distance, _energy;
        private TimeSpan _duration;
        private float _rating;
        private int _participants, _breaks;
        private string _vehicle, _notes, _weather;

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
        /// The time spent travelling.
        /// </summary>
        public TimeSpan Duration
        {
            get => _duration;
            set => _tracker.SetProperty(ref _duration, value);
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
        /// The user's rating of the tour.
        /// </summary>
        public float Rating
        {
            get => _rating;
            set => _tracker.SetProperty(ref _rating, value);
        }

        /// <summary>
        /// The number of people that participated on this tour.
        /// </summary>
        public int ParticipantCount
        {
            get => _participants;
            set => _tracker.SetProperty(ref _participants, value);
        }

        /// <summary>
        /// The number of breaks taken during the tour.
        /// </summary>
        public int BreakCount
        {
            get => _breaks;
            set => _tracker.SetProperty(ref _breaks, value);
        }

        /// <summary>
        /// The amount of energy used during the tour in kWh.
        /// </summary>
        public double EnergyUsed
        {
            get => _energy;
            set => _tracker.SetProperty(ref _energy, value);
        }

        /// <summary>
        /// The vehicle used on this tour.
        /// </summary>
        [MaxLength(64)]
        public string Vehicle
        {
            get => _vehicle ?? String.Empty;
            set => _tracker.SetProperty(ref _vehicle, value);
        }

        /// <summary>
        /// The weather conditions during the tour.
        /// </summary>
        [MaxLength(64)]
        public string Weather
        {
            get => _weather ?? String.Empty;
            set => _tracker.SetProperty(ref _weather, value);
        }

        /// <summary>
        /// The user's notes on this tour.
        /// </summary>
        [MaxLength(2048)]
        public string Notes
        {
            get => _notes ?? String.Empty;
            set => _tracker.SetProperty(ref _notes, value);
        }

        public bool IsChanged => _tracker.IsChanged;

        public void AcceptChanges() => _tracker.AcceptChanges();
    }
}
