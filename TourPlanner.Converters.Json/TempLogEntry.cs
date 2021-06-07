using System;

namespace TourPlanner.Converters.Json
{
    internal class TempLogEntry
    {
        public Guid LogId;
        public DateTime Date;
        public TimeSpan Duration;
        public double Distance;
        public float Rating;
        public int ParticipantCount;
        public int BreakCount;
        public double EnergyUsed;
        public string Vehicle;
        public string Weather;
        public string Notes;
    }
}
