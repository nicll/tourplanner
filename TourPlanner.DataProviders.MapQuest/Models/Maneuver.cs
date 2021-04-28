using System;
using System.Text.Json.Serialization;

namespace TourPlanner.DataProviders.MapQuest.Models
{
    internal readonly struct Maneuver
    {
        public readonly double Distance;

        public readonly string Narrative;

        [JsonConstructor]
        public Maneuver(double distance, string narrative)
        {
            Distance = distance;
            Narrative = narrative;
        }
    }
}
