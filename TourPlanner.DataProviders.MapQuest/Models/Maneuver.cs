using System;
using System.Text.Json.Serialization;

namespace TourPlanner.DataProviders.MapQuest.Models
{
    internal readonly struct Maneuver
    {
        public readonly double Distance;

        public readonly string Narrative;

        public readonly string IconUrl;

        [JsonConstructor]
        public Maneuver(double distance, string narrative, string iconUrl)
        {
            Distance = distance;
            Narrative = narrative;
            IconUrl = iconUrl;
        }
    }
}
