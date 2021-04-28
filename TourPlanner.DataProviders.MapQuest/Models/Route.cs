using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TourPlanner.DataProviders.MapQuest.Models
{
    internal readonly struct Route
    {
        public readonly string SessionId;

        public readonly double Distance;

        public readonly IList<Leg> Legs;

        [JsonConstructor]
        public Route(string sessionId, double distance, IList<Leg> legs)
        {
            SessionId = sessionId;
            Distance = distance;
            Legs = legs;
        }
    }
}
