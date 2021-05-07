using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.Converters.Json
{
    public class JsonConverter : IDataConverter
    {
        private static readonly JsonSerializerOptions _jsonOpts = new();

        public string PreferredFileExtension => "json";

        public async Task<ICollection<Tour>> ReadTours(Stream inputStream)
            => await JsonSerializer.DeserializeAsync<ICollection<Tour>>(inputStream, _jsonOpts).ConfigureAwait(false);

        public async Task WriteTours(Stream outputStream, ICollection<Tour> tours)
            => await JsonSerializer.SerializeAsync(outputStream, tours, _jsonOpts).ConfigureAwait(false);
    }
}
