using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TourPlanner.DataProviders.MapQuest.Models
{
    internal readonly struct Leg
    {
        public readonly IList<Maneuver> Maneuvers;

        [JsonConstructor]
        public Leg(IList<Maneuver> maneuvers)
            => Maneuvers = maneuvers;
    }
}
