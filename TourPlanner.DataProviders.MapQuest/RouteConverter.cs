using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using TourPlanner.Core.Exceptions;
using TourPlanner.Core.Models;
using TourPlanner.DataProviders.MapQuest.Models;
using CoreRoute = TourPlanner.Core.Models.Route;

namespace TourPlanner.DataProviders.MapQuest
{
    internal class RouteConverter : JsonConverter<CoreRoute>
    {
        public override CoreRoute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var wrapper = JsonSerializer.Deserialize<RouteWrapper>(ref reader, options);
            ref readonly var internalRoute = ref wrapper.Route;

            if (internalRoute.SessionId is null)
                throw new DataProviderExcpetion("Received invalid replay from MapQuest.");

            return new CoreRoute { RouteId = internalRoute.SessionId, TotalDistance = internalRoute.Distance, Steps = ManeuversToSteps(internalRoute.Legs[0].Maneuvers) };
        }

        private static IReadOnlyList<Step> ManeuversToSteps(IList<Maneuver> maneuvers)
            => maneuvers.Select(m => new Step { Distance = m.Distance, Description = m.Narrative, IconPath = m.IconUrl }).ToList().AsReadOnly();

        public override void Write(Utf8JsonWriter writer, CoreRoute value, JsonSerializerOptions options)
        {
            var wrapper = new RouteWrapper(new(value.RouteId, value.TotalDistance, new[] { new Leg(StepsToManeuvers(value.Steps)) }));
            JsonSerializer.Serialize(writer, wrapper, options);
        }

        private static List<Maneuver> StepsToManeuvers(IReadOnlyList<Step> steps)
            => steps.Select(s => new Maneuver(s.Distance, s.Description, s.IconPath)).ToList();
    }
}
