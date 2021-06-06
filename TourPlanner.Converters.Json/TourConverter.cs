using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TourPlanner.Core.Models;

namespace TourPlanner.Converters.Json
{
    internal class TourConverter : JsonConverter<Tour>
    {
        public override Tour Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var temp = JsonSerializer.Deserialize<TemporaryTour>(ref reader, options);
            return new Tour(temp.Log) { TourId = temp.TourId, Name = temp.Name, ImagePath = temp.ImagePath, CustomDescription = temp.CustomDescription, Route = temp.Route };
        }

        public override void Write(Utf8JsonWriter writer, Tour value, JsonSerializerOptions options)
        {
            var temp = new TemporaryTour(value.TourId, value.Name, value.ImagePath, value.CustomDescription, value.Route, value.Log);
            JsonSerializer.Serialize(writer, temp, options);
        }
    }
}
