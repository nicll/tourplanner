using System;
using System.Net.Http;
using System.Text.Json;

namespace TourPlanner.DataProviders.MapQuest
{
    internal abstract class MapQuestAPI
    {
        protected readonly JsonSerializerOptions _jsonOpts = new() { IncludeFields = true, PropertyNameCaseInsensitive = true, Converters = { new RouteConverter() } };
        protected readonly HttpClient _client;
        protected readonly string _apiKey;
        protected readonly TimeSpan _timeout;

        internal MapQuestAPI(string apiKey, TimeSpan timeout, HttpClient client)
        {
            _client = client;
            _apiKey = apiKey;
            _timeout = timeout;
        }
    }
}
