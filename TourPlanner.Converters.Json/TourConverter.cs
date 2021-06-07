using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using TourPlanner.Core.Models;

namespace TourPlanner.Converters.Json
{
    internal class TourConverter : JsonConverter<Tour>
    {
        public override Tour Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tour = JsonSerializer.Deserialize<TemporaryTour>(ref reader, options);

            tour.Log.ForEach(l => l.LogId = Guid.NewGuid());
            var log = tour.Log.Select(l => new LogEntry()
            {
                LogId = Guid.NewGuid(),
                Date = l.Date,
                Duration = l.Duration,
                Distance = l.Distance,
                Rating = l.Rating,
                ParticipantCount = l.ParticipantCount,
                BreakCount = l.BreakCount,
                EnergyUsed = l.EnergyUsed,
                Vehicle = l.Vehicle,
                Weather = l.Weather,
                Notes = l.Notes
            }).ToList();

            return new Tour(log)
            {
                TourId = Guid.NewGuid(),
                Name = tour.Name,
                ImagePath = tour.ImagePath,
                CustomDescription = tour.CustomDescription,
                Route = tour.Route
            };
        }

        public override void Write(Utf8JsonWriter writer, Tour value, JsonSerializerOptions options)
        {
            var log = value.Log.Select(l => new TempLogEntry()
            {
                LogId = l.LogId,
                Date = l.Date,
                Duration = l.Duration,
                Distance = l.Distance,
                Rating = l.Rating,
                ParticipantCount = l.ParticipantCount,
                BreakCount = l.BreakCount,
                EnergyUsed = l.EnergyUsed,
                Vehicle = l.Vehicle,
                Weather = l.Weather,
                Notes = l.Notes
            }).ToList();
            var tour = new TemporaryTour(value.TourId, value.Name, value.ImagePath, value.CustomDescription, value.Route, log);

            JsonSerializer.Serialize(writer, tour, options);
        }
    }
}
