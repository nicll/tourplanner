using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DataProviders.MapQuest
{
    internal class Directions : MapQuestAPI, IDirectionsProvider
    {
        private const string DirectionsRequestFormat = "https://www.mapquestapi.com/directions/v2/route?key={0}&from={1}&to={2}&outFormat=json&unit=k&locale=de_DE";
        private readonly Dictionary<(string from, string to), Route> _cachedRoutes = new(new StringValueTupleCurrentCultureCaseInsensitiveComparer());

        internal Directions(string apiKey, TimeSpan timeout, HttpClient client) : base(apiKey, timeout, client)
        {
        }

        public async ValueTask<Route> GetRoute(string from, string to)
        {
            var location = (from, to);

            if (_cachedRoutes.TryGetValue(location, out var route))
                return route;

            route = await FetchRoute(from, to).ConfigureAwait(false);
            _cachedRoutes.Add(location, route);

            return route;
        }

        private async Task<Route> FetchRoute(string from, string to)
        {
            // ToDo: add logging
            var requestUrl = String.Format(DirectionsRequestFormat, _apiKey, from, to);

            using var cts = new CancellationTokenSource(_timeout);
            var response = await _client.GetStreamAsync(requestUrl, cts.Token);

            var route = await JsonSerializer.DeserializeAsync<Route>(response, _jsonOpts) with { StartLocation = from, EndLocation = to };

            return route;
        }
    }
}
