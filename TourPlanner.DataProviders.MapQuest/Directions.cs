using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        { }

        public async ValueTask<Route> GetRoute(string from, string to)
        {
            var location = (from, to);

            if (_cachedRoutes.TryGetValue(location, out var route))
                return route;

            route = await FetchRoute(from, to).ConfigureAwait(false);
            _cachedRoutes.Add(location, route);

            return route;
        }

        public ValueTask CleanCache(IDataManager dataManager)
        {
            foreach (var filePath in Directory.EnumerateFiles(RelativeIconImagesPath))
            {
                var iconName = Path.GetFileNameWithoutExtension(filePath);

                if (dataManager.AllTours.Any(t => t.Route.Steps.Any(s => Path.GetFileNameWithoutExtension(s.IconPath) == iconName)))
                    continue; // skip if still exists

                File.Delete(filePath);
                _log.Debug("Deleted file in image cache: " + filePath);
            }

            _log.Info("Cleared icon cache.");
            return ValueTask.CompletedTask;
        }

        private async Task<Route> FetchRoute(string from, string to)
        {
            _log.Debug("Requesting route from API from=\"" + from + "\" to=\"" + to + "\"");
            var requestUrl = String.Format(DirectionsRequestFormat, _apiKey, from, to);
            using var cts = new CancellationTokenSource(_timeout);
            var response = await _client.GetAsync(requestUrl, cts.Token);
            _log.Debug("Received response from API.");
            response.EnsureSuccessStatusCode();

            var route = await JsonSerializer.DeserializeAsync<Route>(await response.Content.ReadAsStreamAsync(), _jsonOpts) with { StartLocation = from, EndLocation = to };
            _log.Info("Received proper response from API: RouteId=" + route.RouteId);

            await FetchIcons(route);

            return route;
        }

        private async Task FetchIcons(Route route)
        {
            EnsureDirectoryExists(RelativeIconImagesPath);
            _log.Info("Requesting icons for route: " + route.RouteId);

            var newSteps = new List<Step>();
            foreach (var step in route.Steps)
            {
                var localPath = MapQuestIconPathToLocalPath(step.IconPath);

                if (!File.Exists(localPath))
                {
                    _log.Debug("Fetching icon: " + step.IconPath);
                    var response = await _client.GetAsync(step.IconPath);
                    response.EnsureSuccessStatusCode();
                    var iconData = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(localPath, iconData);
                    _log.Debug("Saved icon to: " + localPath);
                }

                newSteps.Add(step with { IconPath = localPath });
            }

            static string MapQuestIconPathToLocalPath(string mqPath)
                => RelativeIconImagesPath + mqPath[(mqPath.LastIndexOf('/') + 1)..];
        }
    }
}
