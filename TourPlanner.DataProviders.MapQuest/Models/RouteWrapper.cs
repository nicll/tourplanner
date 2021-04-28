using System;
using System.Text.Json.Serialization;

namespace TourPlanner.DataProviders.MapQuest.Models
{
    internal readonly struct RouteWrapper
    {
        public readonly Route Route;

        [JsonConstructor]
        public RouteWrapper(Route route)
            => Route = route;
    }
}
